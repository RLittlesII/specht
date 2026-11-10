using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Consumption;

/// <summary>
/// Offline package-consumption checks for 0055-F7. A folder feed above the consumer
/// repository stands in for nuget.org. The repository's own pin is verified separately against the public feed.
/// </summary>
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

    [Given("an empty repository with no package source of its own")]
    public void GivenAnEmptyRepositoryWithNoPackageSourceOfItsOwn()
    {
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

    [Given("a clone of this repository with its committed checker pin")]
    public void GivenACloneOfThisRepositoryWithItsCommittedCheckerPin()
    {
        ReadManifest();
        var checker = _manifest!["tools"]!["specht.tool"]!;
        _feedVersion = checker["version"]!.GetValue<string>();
        if (_feedVersion.StartsWith("0.", StringComparison.Ordinal))
        {
            _feedVersion.Should().EndWith("-preview");
        }

        _manifest["tools"] = new JsonObject { ["specht.tool"] = checker.DeepClone() };
    }

    [Given("a clone of this repository after the first publish")]
    public void GivenACloneOfThisRepositoryAfterTheFirstPublish()
    {
        ReadManifest();
        _configuration = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Consumption/nuget.config"));
    }

    [When("the prerelease tool is installed locally")]
    public async Task WhenThePrereleaseToolIsInstalledLocally()
    {
        await Prepare();
        File.Exists(Path.Combine(Repository, "nuget.config")).Should().BeFalse();
        File.Exists(Path.Combine(Repository, ".config/dotnet-tools.json")).Should().BeFalse();
        _result = await Run(Repository, "new", "tool-manifest", "--output", ".config");
        _result.ExitCode.Should().Be(0, _result.Output);
        _result = await Run(Repository, "tool", "install", "--local", "specht.tool", "--prerelease");
    }

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

    [Then("the tool is available at the pinned version")]
    public async Task ThenTheToolIsAvailableAtThePinnedVersion()
    {
        _result.ExitCode.Should().Be(0, _result.Output);
        var (ExitCode, Output) = await Run(Repository, "tool", "run", "specht");
        ExitCode.Should().Be(0, Output);
        Output.Trim().Should().Be(_manifest!["tools"]!["specht.tool"]!["version"]!.GetValue<string>());
    }

    [Then("its committed package configuration names only nuget.org")]
    public void ThenItsCommittedPackageConfigurationNamesOnlyNuGetOrg()
    {
        var root = _configuration!.Root!;
        root.Element("packageSources")!.Elements("add").Select(static source => (string?) source.Attribute("value"))
            .Should().Equal("https://api.nuget.org/v3/index.json");
        root.Element("packageSourceMapping")!.Elements("packageSource").Select(static source => (string?) source.Attribute("key"))
            .Should().Equal("nuget.org");
    }

    [Then("the build succeeds")]
    public void ThenTheBuildSucceeds() => _result.ExitCode.Should().Be(0, _result.Output);

    private string Repository => Path.Combine(_root, "repository");

    private void ReadManifest()
    {
        _manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Consumption/dotnet-tools.json")))!;
        _manifest["tools"]!.AsObject().Should().ContainKey("specht.tool", "the published checker must be pinned before consumption");
    }

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
            Directory.CreateDirectory(Path.Combine(Repository, "feeds/public"));
            source.SetAttributeValue("value", "feeds/public");
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
    private string? _feedVersion;
    private bool _withoutCredential;
    private bool _install;
    private XDocument? _configuration;
    private (int ExitCode, string Output) _result;
}
