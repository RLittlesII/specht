using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Consumption;

/// <summary>
/// Installation documentation and offline package-consumption proofs for 0055-F7. A folder feed above the consumer
/// repository, configured as the only source as a machine's default nuget.org is, stands in for nuget.org. B-010 and
/// B-011 state the end of the feed transition, item 0328, and stay unbound until it is true.
/// </summary>
[Binding]
[Scope(Feature = "Consuming the package")]
public sealed class ConsumptionSteps(PackedTool tool) : IDisposable
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

    [Given("an empty repository with no package source of its own")]
    public void GivenAnEmptyRepositoryWithNoPackageSourceOfItsOwn()
    {
        ReadInstallDocument();
        _feedVersion = SyntheticVersion;
        _install = true;
    }

    [Given("no local tool manifest exists")]
    public void GivenNoLocalToolManifestExists() => File.Exists(Path.Combine(Repository, ".config/dotnet-tools.json")).Should().BeFalse();

    [Given("no credential for any package feed")]
    [Given("no token for the package feed")]
    public void GivenNoCredentialForAnyPackageFeed() => _withoutCredential = true;

    [Given("a clone whose committed tool manifest names the tool at a version published to nuget.org")]
    public void GivenACloneWhoseCommittedToolManifestNamesTheToolAtAVersionPublishedToNuGetOrg()
    {
        _feedVersion = SyntheticVersion;
        _manifest = new JsonObject
        {
            ["version"] = 1,
            ["isRoot"] = true,
            ["tools"] = new JsonObject
            {
                ["specht.tool"] = new JsonObject
                {
                    ["version"] = SyntheticVersion,
                    ["commands"] = new JsonArray("specht"),
                    ["rollForward"] = false,
                },
            },
        };
    }

    [Given("a clone of this repository after the first publish")]
    public void GivenACloneOfThisRepositoryAfterTheFirstPublish()
    {
        ReadManifest();
        _configuration = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Consumption/nuget.config"));
    }

    [Given("every published version of the tool is below 1.0.0")]
    public void GivenEveryPublishedVersionOfTheToolIsBelowOne() =>
        JsonNode.Parse(File.ReadAllText(Path.Combine(tool.Repository, "version.json")))!["version"]!.GetValue<string>()
            .Should().StartWith("0.");

    [When("the documented install command runs")]
    public async Task WhenTheDocumentedInstallCommandRuns()
    {
        await Prepare();
        File.Exists(Path.Combine(Repository, "nuget.config")).Should().BeFalse();
        File.Exists(Path.Combine(Repository, ".config/dotnet-tools.json")).Should().BeFalse();
        _result = await Run(Repository, DocumentArguments("dotnet new tool-manifest"));
        _result.ExitCode.Should().Be(0, _result.Output);
        _result = await Run(Repository, DocumentArguments("dotnet tool install --local "));
    }

    [When("a consumer reads the install document")]
    public void WhenAConsumerReadsTheInstallDocument() => ReadInstallDocument();

    [When("the local tools are restored")]
    public async Task WhenTheLocalToolsAreRestored()
    {
        await Prepare();
        _result = await Run(Repository, "tool", "restore");
    }

    [When("the solution is restored and built")]
    public async Task WhenTheSolutionIsRestoredAndBuilt()
    {
        await Prepare();
        _result = await Run(Repository, "restore", "fixture.slnx");
        _result.ExitCode.Should().Be(0, _result.Output);
        _result = await Run(Repository, "build", "fixture.slnx", "--no-restore");
    }

    [Then("the tool is added to the repository's local tool manifest")]
    public void ThenTheToolIsAddedToTheRepositorysLocalToolManifest()
    {
        _result.ExitCode.Should().Be(0, _result.Output);
        var installed = JsonNode.Parse(File.ReadAllText(Path.Combine(Repository, ".config/dotnet-tools.json")))!;
        installed["tools"]!["specht.tool"]!["version"]!.GetValue<string>().Should().Be(_feedVersion);
        installed["tools"]!["specht.tool"]!["commands"]!.AsArray()
            .Select(static command => command!.GetValue<string>()).Should().Equal("specht");
    }

    [Then("it names nuget.org as the feed")]
    public void ThenItNamesNuGetOrgAsTheFeed() =>
        _installDocument.Should().Contain("[nuget.org](https://www.nuget.org/packages/specht.tool)");

    [Then("says no package source entry is committed for it")]
    public void ThenSaysNoPackageSourceEntryIsCommittedForIt()
    {
        Prose.Should().Contain("No package source entry is committed for it");
        _installDocument.Should().NotContain("<packageSources>");
    }

    [Then("it says installing and restoring the tool needs no credential")]
    public void ThenItSaysInstallingAndRestoringTheToolNeedsNoCredential()
    {
        Prose.Should().Contain("no credential is needed to install or restore it");
        _installDocument.Should().NotContain("NuGetPackageSourceCredentials");
    }

    [Then("it names the command that creates the local tool manifest")]
    public void ThenItNamesTheCommandThatCreatesTheLocalToolManifest() =>
        DocumentCommand("dotnet new tool-manifest").Should().Be("dotnet new tool-manifest --output .config");

    [Then("it names the command that installs the tool locally")]
    public void ThenItNamesTheCommandThatInstallsTheToolLocally() =>
        DocumentCommand("dotnet tool install --local ").Should().MatchRegex(@"^dotnet tool install --local specht\.tool( |$)");

    [Then("the command that restores it in a clone")]
    public void ThenTheCommandThatRestoresItInAClone() => DocumentCommand("dotnet tool restore").Should().Be("dotnet tool restore");

    [Then("it names the command that installs the tool globally")]
    public void ThenItNamesTheCommandThatInstallsTheToolGlobally() =>
        DocumentCommand("dotnet tool install --global ").Should().MatchRegex(@"^dotnet tool install --global specht\.tool( |$)");

    [Then("names no package source or credential for it")]
    public void ThenNamesNoPackageSourceOrCredentialForIt()
    {
        DocumentCommand("dotnet tool install --global ").Should().NotContain("--add-source").And.NotContain("--configfile");
        _installDocument.Should().NotContain("dotnet nuget add source").And.NotContain("NuGetPackageSourceCredentials");
    }

    [Then("says a global install puts the command on the shell's path")]
    public void ThenSaysAGlobalInstallPutsTheCommandOnTheShellsPath() => _installDocument.Should().Contain("`specht` on PATH");

    [Then("says a tool manifest install is run through dotnet")]
    public void ThenSaysAToolManifestInstallIsRunThroughDotnet() => _installDocument.Should().Contain("`dotnet specht`");

    [Then("each install command it names opts in to prerelease versions")]
    public void ThenEachInstallCommandItNamesOptsInToPrereleaseVersions() =>
        _installDocument.Split('\n').Select(static line => line.Trim())
            .Where(static line => line.StartsWith("dotnet tool install ", StringComparison.Ordinal))
            .Should().NotBeEmpty()
            .And.AllSatisfy(static command => command.Split(' ').Should().Contain("--prerelease"));

    [Then("the tool is available at the pinned version")]
    public async Task ThenTheToolIsAvailableAtThePinnedVersion()
    {
        _result.ExitCode.Should().Be(0, _result.Output);
        var (ExitCode, Output) = await Run(Repository, "tool", "run", "specht");
        ExitCode.Should().Be(0, Output);
        Output.Trim().Should().Be(_manifest!["tools"]!["specht.tool"]!["version"]!.GetValue<string>());
    }

    [Then("this repository's committed tool manifest names the tool at a published version")]
    public void ThenThisRepositorysCommittedToolManifestNamesTheToolAtAPublishedVersion()
    {
        ReadManifest();
        var checker = _manifest!["tools"]!["specht.tool"]!;
        checker["version"]!.GetValue<string>().Should().BeOneOf(_publishedVersion!, "0.1.131");
        checker["commands"]!.AsArray().Select(static command => command!.GetValue<string>()).Should().Equal("specht");
    }

    [Then("the build succeeds")]
    public void ThenTheBuildSucceeds() => _result.ExitCode.Should().Be(0, _result.Output);

    private string Repository => Path.Combine(_root, "repository");

    private string Prose => Regex.Replace(_installDocument, @"\s+", " ");

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

    private string DocumentCommand(string prefix) => _installDocument.Split('\n').Select(static line => line.Trim())
        .Single(line => line.StartsWith(prefix, StringComparison.Ordinal));

    private string[] DocumentArguments(string prefix) => [.. DocumentCommand(prefix).Split(' ').Skip(1)];

    private async Task Prepare()
    {
        Directory.CreateDirectory(Path.Combine(_root, "feeds/nuget.org"));
        await Write("nuget.config", """
            <configuration>
              <packageSources><clear /><add key="nuget.org" value="feeds/nuget.org" /></packageSources>
              <packageSourceMapping><clear /></packageSourceMapping>
              <auditSources><clear /></auditSources>
            </configuration>
            """);
        await Write("Directory.Build.props", "<Project />");
        await Write("Directory.Packages.props", "<Project />");
        Directory.CreateDirectory(Repository);
        if (!_install && _manifest is not null)
        {
            await Write("repository/.config/dotnet-tools.json", _manifest.ToJsonString());
        }

        if (_configuration is not null)
        {
            await WriteClone(_configuration);
        }

        if (_feedVersion is null)
        {
            return;
        }

        await Write("package/package.csproj", """
            <Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
              <TargetFramework>net$(BundledNETCoreAppTargetFrameworkVersion)</TargetFramework><OutputType>Exe</OutputType>
              <PackAsTool>true</PackAsTool><NuGetAudit>false</NuGetAudit>
              <IncludeSourceRevisionInInformationalVersion>false</IncludeSourceRevisionInInformationalVersion>
            </PropertyGroup></Project>
            """);
        await Write("package/Program.cs", """
            System.Console.WriteLine(System.Reflection.CustomAttributeExtensions
                .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(System.Reflection.Assembly.GetExecutingAssembly())!
                .InformationalVersion);
            """);
        var (ExitCode, Output) = await Run(_root, "pack", "package/package.csproj", "--output", "feeds/nuget.org",
            "-p:PackageId=specht.tool", "-p:Version=" + _feedVersion, "-p:ToolCommandName=specht");
        ExitCode.Should().Be(0, Output);
    }

    private async Task WriteClone(XDocument configuration)
    {
        foreach (var source in configuration.Root!.Element("packageSources")!.Elements("add"))
        {
            var feed = (string?) source.Attribute("key") == "specht" ? "feeds/checker" : "feeds/public";
            Directory.CreateDirectory(Path.Combine(Repository, feed));
            source.SetAttributeValue("value", feed);
        }

        foreach (var source in configuration.Root!.Element("auditSources")!.Elements("add"))
        {
            source.SetAttributeValue("value", "feeds/public");
        }

        await Write("repository/nuget.config", configuration.ToString());
        await Write("repository/fixture.slnx", "<Solution><Project Path=\"app/app.csproj\" /></Solution>");
        await Write("repository/app/app.csproj", """
            <Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net$(BundledNETCoreAppTargetFrameworkVersion)</TargetFramework>
            <NuGetAudit>false</NuGetAudit></PropertyGroup></Project>
            """);
        await Write("repository/app/Class.cs", "public sealed class SyntheticApplication { }");
    }

    private async Task Write(string path, string text)
    {
        var file = Path.Combine(_root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        await File.WriteAllTextAsync(file, text, TestContext.Current.CancellationToken);
    }

    private async Task<(int ExitCode, string Output)> Run(string directory, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Path.GetFullPath(directory),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.Environment.Remove("GITHUB_ACTIONS");
        foreach (var key in start.Environment.Keys.Where(key => _withoutCredential &&
            (key.StartsWith("NuGetPackageSourceCredentials_", StringComparison.OrdinalIgnoreCase) || key is "GITHUB_TOKEN" or "GH_TOKEN")).ToArray())
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

    private const string SyntheticVersion = "0.9.4-preview";

    private readonly string _root = Path.Combine(".artifacts", "consumption-tests", Guid.NewGuid().ToString("N"));
    private JsonNode? _manifest;
    private string? _publishedVersion;
    private string? _feedVersion;
    private bool _withoutCredential;
    private bool _install;
    private string _installDocument = string.Empty;
    private XDocument? _configuration;
    private (int ExitCode, string Output) _result;
}
