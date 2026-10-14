using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Consumption;

/// <summary>Installation documentation and offline package-consumption proofs for 0055-F7.</summary>
[Binding]
[Scope(Feature = "Consuming the package")]
public sealed class ConsumptionSteps : IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Given("the first package has been published")]
    public void GivenTheFirstPackageHasBeenPublished() => _publishedVersion = "0.1.90";

    [Given("a clone whose committed tool manifest names the tool")]
    public void GivenACloneWhoseCommittedToolManifestNamesTheTool() => ReadManifest();

    [Given("whose committed package source is the documented one")]
    public void GivenWhoseCommittedPackageSourceIsTheDocumentedOne()
    {
        var sources = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Consumption/nuget.config"));
        sources.Root!.Element("packageSources")!.Elements("add")
            .Single(static source => (string?)source.Attribute("key") == "specht")
            .Attribute("value")!.Value.Should().Be("https://nuget.pkg.github.com/rlittlesii/index.json");
    }

    [Given("a token allowed to read packages")]
    public void GivenATokenAllowedToReadPackages() => _credential = true;

    [Given("a clone of this repository after the first publish")]
    public void GivenACloneOfThisRepositoryAfterTheFirstPublish() => ReadManifest();

    [Given("no token for the package feed")]
    public void GivenNoTokenForThePackageFeed() => _credential = false;

    [Given("an empty repository with the documented package source")]
    public void GivenAnEmptyRepositoryWithTheDocumentedPackageSource()
    {
        ReadInstallDocument();
        _documentSource = ReadDocumentSource();
        ReadManifest();
        foreach (var name in _manifest!["tools"]!.AsObject().Select(static tool => tool.Key).Where(static name => name != "specht.tool").ToArray())
        {
            _manifest["tools"]!.AsObject().Remove(name);
        }

        _install = true;
    }

    [Given("no local tool manifest exists")]
    public void GivenNoLocalToolManifestExists() => File.Exists(Path.Combine(_root, ".config/dotnet-tools.json")).Should().BeFalse();

    [When("the documented install command runs")]
    public async Task WhenTheDocumentedInstallCommandRuns()
    {
        await Prepare();
        File.Exists(Path.Combine(_root, ".config/dotnet-tools.json")).Should().BeFalse();
        _result = await Run(DocumentCommand("dotnet new tool-manifest").Split(' ').Skip(1).ToArray());
        _result.ExitCode.Should().Be(0, _result.Output);
        _result = await Run(DocumentCommand("dotnet tool install --local ").Split(' ').Skip(1).ToArray());
    }

    [When("a consumer reads the install document")]
    public void WhenAConsumerReadsTheInstallDocument() => ReadInstallDocument();

    [Then("the tool is added to the repository's local tool manifest")]
    public void ThenTheToolIsAddedToTheRepositorysLocalToolManifest()
    {
        _result.ExitCode.Should().Be(0, _result.Output);
        var installed = JsonNode.Parse(File.ReadAllText(Path.Combine(_root, ".config/dotnet-tools.json")))!;
        installed["tools"]!["specht.tool"]!["version"]!.GetValue<string>()
            .Should().Be(_manifest!["tools"]!["specht.tool"]!["version"]!.GetValue<string>());
        installed["tools"]!["specht.tool"]!["commands"]!.AsArray()
            .Select(static command => command!.GetValue<string>()).Should().Equal("specht");
    }

    [Then("it names the feed's address")]
    public void ThenItNamesTheFeedsAddress() => _installDocument.Should().Contain("https://nuget.pkg.github.com/rlittlesii/index.json");

    [Then("the package source entry to commit")]
    public void ThenThePackageSourceEntryToCommit()
    {
        var source = ReadDocumentSource();
        source.Root!.Element("packageSources")!.Elements("add")
            .Single(static entry => (string?)entry.Attribute("key") == "specht")
            .Attribute("value")!.Value.Should().Be("https://nuget.pkg.github.com/rlittlesii/index.json");
        source.Descendants("packageSourceCredentials").Should().BeEmpty();
        _installDocument.Should().Contain("nuget.config").And.Contain("Commit");
    }

    [Then("it names the permission the token needs")]
    public void ThenItNamesThePermissionTheTokenNeeds() =>
        _installDocument.Should().Contain("personal access token (classic)").And.Contain("read:packages");

    [Then("how a CI run supplies the token")]
    public void ThenHowACIRunSuppliesTheToken() => _installDocument.Should()
        .Contain("NuGetPackageSourceCredentials_specht:")
        .And.Contain("${{ secrets.SPECHT_PACKAGES_TOKEN }}")
        .And.Contain("${{ github.token }}")
        .And.Contain("packages: read");

    [Then("it names the command that creates the local tool manifest")]
    public void ThenItNamesTheCommandThatCreatesTheLocalToolManifest() =>
        DocumentCommand("dotnet new tool-manifest").Should().Be("dotnet new tool-manifest --output .config");

    [Then("it names the command that installs the tool locally")]
    public void ThenItNamesTheCommandThatInstallsTheToolLocally() =>
        DocumentCommand("dotnet tool install --local ").Should().Be("dotnet tool install --local specht.tool");

    [Then("the command that restores it in a clone")]
    public void ThenTheCommandThatRestoresItInAClone() => DocumentCommand("dotnet tool restore").Should().Be("dotnet tool restore");

    [Then("it names the command that installs the tool globally")]
    public void ThenItNamesTheCommandThatInstallsTheToolGlobally() => DocumentCommand("dotnet tool install --global ").Should()
        .Be("dotnet tool install --global specht.tool --add-source https://nuget.pkg.github.com/rlittlesii/index.json");

    [Then("names the source and environment credential for a shell outside a repository")]
    public void ThenNamesTheSourceAndEnvironmentCredentialForAShellOutsideARepository()
    {
        DocumentCommand("dotnet nuget add source ").Should()
            .Be("dotnet nuget add source https://nuget.pkg.github.com/rlittlesii/index.json --name specht");
        _installDocument.Should().Contain("user-level").And.Contain("outside a repository")
            .And.Contain("export NuGetPackageSourceCredentials_specht=")
            .And.Contain("Username=$GITHUB_USER;Password=$SPECHT_PACKAGES_TOKEN;ValidAuthenticationTypes=Basic");
    }

    [Then("says a global install puts the command on the shell's path")]
    public void ThenSaysAGlobalInstallPutsTheCommandOnTheShellsPath() => _installDocument.Should().Contain("`specht` on PATH");

    [Then("says a tool manifest install is run through dotnet")]
    public void ThenSaysAToolManifestInstallIsRunThroughDotnet() => _installDocument.Should().Contain("`dotnet specht`");

    [When("the local tools are restored")]
    public async Task WhenTheLocalToolsAreRestored()
    {
        await Prepare();
        _result = await Run("tool", "restore");
    }

    [When("the solution is restored and built")]
    public async Task WhenTheSolutionIsRestoredAndBuilt()
    {
        await Prepare();
        _result = await Run("restore", "fixture.slnx");
        _result.ExitCode.Should().Be(0, _result.Output);
        _result = await Run("build", "fixture.slnx", "--no-restore");
    }

    [Then("the tool is available at the pinned version")]
    public async Task ThenTheToolIsAvailableAtThePinnedVersion()
    {
        _result.ExitCode.Should().Be(0, _result.Output);
        var result = await Run("tool", "run", "specht");
        result.ExitCode.Should().Be(0, result.Output);
        result.Output.Trim().Should().Be(_manifest!["tools"]!["specht.tool"]!["version"]!.GetValue<string>());
    }

    [Then("this repository's committed tool manifest names the tool at a published version")]
    public void ThenThisRepositorysCommittedToolManifestNamesTheToolAtAPublishedVersion()
    {
        ReadManifest();
        var checker = _manifest!["tools"]!["specht.tool"]!;
        checker["version"]!.GetValue<string>().Should().Be(_publishedVersion);
        checker["commands"]!.AsArray().Select(static command => command!.GetValue<string>()).Should().Equal("specht");
    }

    [Then("the build succeeds")]
    public void ThenTheBuildSucceeds() => _result.ExitCode.Should().Be(0, _result.Output);

    private void ReadManifest()
    {
        _manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Consumption/dotnet-tools.json")))!;
        _manifest["tools"]!.AsObject().Should().ContainKey("specht.tool", "the published checker must be pinned before consumption");
    }

    private void ReadInstallDocument()
    {
        var readme = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "README.md"));
        var start = readme.IndexOf("## Quick start", StringComparison.Ordinal);
        var end = readme.IndexOf("## Usage", StringComparison.Ordinal);
        start.Should().BeGreaterThanOrEqualTo(0);
        end.Should().BeGreaterThan(start);
        _installDocument = readme[start..end];
    }

    private XDocument ReadDocumentSource() =>
        XDocument.Parse(Regex.Match(_installDocument, "<configuration>.*?</configuration>", RegexOptions.Singleline).Value);

    private string DocumentCommand(string prefix) => _installDocument.Split('\n').Select(static line => line.Trim())
        .Single(line => line.StartsWith(prefix, StringComparison.Ordinal));

    private async Task Prepare()
    {
        Directory.CreateDirectory(Path.Combine(_root, ".config"));
        Directory.CreateDirectory(Path.Combine(_root, "feeds/public"));
        if (!_install)
        {
            await Write(".config/dotnet-tools.json", _manifest!.ToJsonString());
        }

        var config = _documentSource ?? XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Consumption/nuget.config"));
        foreach (var source in config.Root!.Element("packageSources")!.Elements("add"))
        {
            source.SetAttributeValue("value", (string?)source.Attribute("key") == "specht" ? "feeds/checker" : "feeds/public");
        }

        foreach (var source in config.Root!.Element("auditSources")!.Elements("add"))
        {
            source.SetAttributeValue("value", "feeds/public");
        }

        await Write("nuget.config", config.ToString());
        await Write("Directory.Build.props", "<Project />");
        await Write("Directory.Packages.props", "<Project />");
        await Write("fixture.slnx", "<Solution><Project Path=\"app/app.csproj\" /></Solution>");
        await Write("app/app.csproj", """
            <Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>
            <NuGetAudit>false</NuGetAudit></PropertyGroup></Project>
            """);
        await Write("app/Class.cs", "public sealed class SyntheticApplication { }");
        if (!_credential)
        {
            return;
        }

        await Write("package/package.csproj", """
            <Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
              <TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType>
              <PackAsTool>true</PackAsTool><NuGetAudit>false</NuGetAudit>
            </PropertyGroup></Project>
            """);
        await Write("package/Program.cs", """
            System.Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString(3));
            """);
        foreach (var tool in _manifest!["tools"]!.AsObject())
        {
            var source = tool.Key == "specht.tool" ? "checker" : "public";
            Directory.CreateDirectory(Path.Combine(_root, "feeds", source));
            var result = await Run("pack", "package/package.csproj", "--output", "feeds/" + source,
                "-p:PackageId=" + tool.Key, "-p:Version=" + tool.Value!["version"]!.GetValue<string>(),
                "-p:ToolCommandName=" + tool.Value["commands"]![0]!.GetValue<string>());
            result.ExitCode.Should().Be(0, result.Output);
        }
    }

    private async Task Write(string path, string text)
    {
        var file = Path.Combine(_root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        await File.WriteAllTextAsync(file, text, TestContext.Current.CancellationToken);
    }

    private async Task<(int ExitCode, string Output)> Run(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.GetFullPath(_root),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var key in start.Environment.Keys.Where(static key =>
            key.StartsWith("NuGetPackageSourceCredentials_", StringComparison.OrdinalIgnoreCase)
            || key is "GITHUB_TOKEN" or "GH_TOKEN" or "GITHUB_ACTIONS").ToArray())
        {
            start.Environment.Remove(key);
        }

        start.Environment["DOTNET_CLI_HOME"] = Path.GetFullPath(Path.Combine(_root, "cli-home"));
        start.Environment["NUGET_PACKAGES"] = Path.GetFullPath(Path.Combine(_root, "packages"));
        start.Environment["NUGET_HTTP_CACHE_PATH"] = Path.GetFullPath(Path.Combine(_root, "http-cache"));
        start.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        start.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        start.Environment["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "1";
        start.Environment["NUKE_TELEMETRY_OPTOUT"] = "1";
        if (_credential)
        {
            start.Environment["NuGetPackageSourceCredentials_specht"] = "Username=synthetic;Password=synthetic-read-token;ValidAuthenticationTypes=Basic";
        }

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, await output + await error);
    }

    private readonly string _root = Path.Combine(".artifacts", "consumption-tests", Guid.NewGuid().ToString("N"));
    private JsonNode? _manifest;
    private string? _publishedVersion;
    private bool _credential;
    private bool _install;
    private string _installDocument = string.Empty;
    private XDocument? _documentSource;
    private (int ExitCode, string Output) _result;
}
