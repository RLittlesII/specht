using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Consumption;

/// <summary>Offline package restore and token-free build proofs for 0055-F7 B-002, B-003 and B-007.</summary>
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

    private async Task Prepare()
    {
        Directory.CreateDirectory(Path.Combine(_root, ".config"));
        Directory.CreateDirectory(Path.Combine(_root, "feeds/public"));
        await Write(".config/dotnet-tools.json", _manifest!.ToJsonString());
        var config = XDocument.Load(Path.Combine(AppContext.BaseDirectory, "Consumption/nuget.config"));
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
        foreach (var tool in _manifest["tools"]!.AsObject())
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
    private (int ExitCode, string Output) _result;
}
