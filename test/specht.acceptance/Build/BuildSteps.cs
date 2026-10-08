using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Reqnroll.UnitTestProvider;

namespace specht.acceptance.Build;

/// <summary>
/// Steps for <c>.build/.spec/build.feature</c> (0055-F1). Each scenario writes a synthetic tree to a
/// temporary directory and runs this repository's real entry script against it with <c>--root</c>,
/// so no scenario runs this repository's own build from inside its own Test target.
/// </summary>
[Binding]
public sealed partial class BuildSteps(IUnitTestRuntimeProvider runtime) : IDisposable
{
    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Given("a fresh clone with the pinned SDK")]
    public void GivenAFreshCloneWithThePinnedSdk()
    {
        WriteTree(("fixture.lib", ClassLibrary), ("fixture.tests", TestProject));
        Write("fixture.tests/Tests.cs", UnitTestSource("Passes", "unit", passes: true));
    }

    [Given("a clone whose tests fail")]
    public void GivenACloneWhoseTestsFail()
    {
        WriteTree(("fixture.tests", TestProject));
        Write("fixture.tests/Tests.cs", UnitTestSource("Fails", "unit", passes: false));
    }

    [Given("a clone on Windows")]
    public void GivenACloneOnWindows()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            runtime.TestIgnore("build.cmd runs on Windows only; this scenario runs on the Windows leg of CI.");
        }

        WriteTree();
    }

    [Given("a failing test in the integration tier only")]
    public void GivenAFailingTestInTheIntegrationTierOnly()
    {
        WriteTree(("fixture.tests", TestProject), ("fixture.acceptance", TestProject));
        Write("fixture.tests/UnitTests.cs", UnitTestSource("Passes", "unit", passes: true));
        Write("fixture.tests/IntegrationTests.cs", UnitTestSource("Fails", "integration", passes: false));
        Write("fixture.acceptance/AcceptanceTests.cs", UnitTestSource("Passes", "acceptance", passes: true));
    }

    [Given("a fresh clone on a machine with no tools installed")]
    public void GivenAFreshCloneOnAMachineWithNoToolsInstalled()
    {
        // NuGet's own package folder is the only source, so the restore never touches the network;
        // the Restore target has already restored this manifest's tools into it.
        Directory.CreateDirectory(Path.Combine(_root, ".config"));
        File.Copy(Path.Combine(Repository, ".config", "dotnet-tools.json"), Path.Combine(_root, ".config", "dotnet-tools.json"));
        Write("nuget.config", $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="local" value="{GlobalPackagesFolder()}" />
              </packageSources>
            </configuration>
            """);
    }

    [Given("a token allowed to read packages once the tool manifest names the checker")]
    public void GivenATokenAllowedToReadPackagesOnceTheToolManifestNamesTheChecker() =>
        ManifestTools().Keys.Should().NotContain(
            "specht.tool",
            "until 0087 names the checker the restore needs no token; 0087 supplies one here");

    [Given("the checker's check command does not exist yet")]
    public void GivenTheCheckersCheckCommandDoesNotExistYet() => WriteTree();

    [When("the build runs with no target named")]
    public async Task WhenTheBuildRunsWithNoTargetNamed() => await RunAsync(UnixEntry());

    [When("a target is run through the Windows entry script")]
    public async Task WhenATargetIsRunThroughTheWindowsEntryScript() => await RunAsync(WindowsEntry("SpecCheck"));

    [When("the test gate runs")]
    public async Task WhenTheTestGateRuns() => await RunAsync(UnixEntry("Test"));

    [When("the pack target runs")]
    public async Task WhenThePackTargetRuns()
    {
        WriteTree(("specht.tool", ToolProject));
        Write("specht.tool/Program.cs", "return 0;");
        await RunAsync(UnixEntry("Pack"));
    }

    [When("the local tools are restored")]
    public async Task WhenTheLocalToolsAreRestored() => await RunAsync(("dotnet", ["tool", "restore"]));

    [When("the self-check runs")]
    public async Task WhenTheSelfCheckRuns() => await RunAsync(UnixEntry("SpecCheck"));

    [Then("the solution is compiled")]
    public void ThenTheSolutionIsCompiled() => Summary().Should().ContainInOrder("Restore Succeeded", "Compile Succeeded");

    [Then("the tests run after it")]
    public void ThenTheTestsRunAfterIt()
    {
        Summary().Should().ContainInOrder("Compile Succeeded", "Test Succeeded");
        _output.Should().Contain("Test run summary: Passed!");
    }

    [Then("the build fails")]
    public void ThenTheBuildFails() => _exitCode.Should().NotBe(0, _output);

    [Then("the same target runs as through the Unix entry script")]
    public void ThenTheSameTargetRunsAsThroughTheUnixEntryScript()
    {
        _exitCode.Should().Be(0, _output);
        Summary().Should().Equal("SpecCheck Succeeded");
    }

    [Then("the unit, integration and acceptance tiers each ran")]
    public void ThenTheUnitIntegrationAndAcceptanceTiersEachRan() =>
        Directory.GetFiles(Markers).Select(Path.GetFileName).Should().BeEquivalentTo("unit", "integration", "acceptance");

    [Then("the test gate fails")]
    public void ThenTheTestGateFails()
    {
        _exitCode.Should().NotBe(0, _output);
        Summary().Should().Contain("Test Failed");
    }

    [Then("exactly one tool package is written to the build's package output")]
    public void ThenExactlyOneToolPackageIsWrittenToTheBuildsPackageOutput()
    {
        _exitCode.Should().Be(0, _output);
        Packages().Should().ContainSingle().Which.Should().StartWith("specht.tool.");
    }

    [Then("its name carries the version computed for the commit")]
    public async Task ThenItsNameCarriesTheVersionComputedForTheCommit()
    {
        await RunAsync(("dotnet", ["msbuild", "specht.tool/specht.tool.csproj", "-getProperty:Version"]));
        Packages().Should().Equal($"specht.tool.{_output.Trim()}.nupkg");
    }

    [Then("every dotnet tool the build and the hook invoke is available")]
    public async Task ThenEveryDotnetToolTheBuildAndTheHookInvokeIsAvailable()
    {
        _exitCode.Should().Be(0, _output);

        // The hook is installed and run by Husky.Net; the build invokes no other dotnet tool.
        await RunAsync(("dotnet", ["tool", "list", "--local"]));
        ListedTools().Keys.Should().Contain("husky");
    }

    [Then("each at the version the committed tool manifest pins")]
    public void ThenEachAtTheVersionTheCommittedToolManifestPins() =>
        ListedTools().Should().BeEquivalentTo(ManifestTools());

    [Then("it reports that the check is not yet available")]
    public void ThenItReportsThatTheCheckIsNotYetAvailable() => _output.Should().Contain("the check is not yet available");

    [Then("it succeeds")]
    public void ThenItSucceeds() => _exitCode.Should().Be(0, _output);

    private static string Repository { get; } = FindRepository(AppContext.BaseDirectory);

    private string Markers => Path.Combine(_root, ".markers");

    private static (string, string[]) UnixEntry(params string[] targets) =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? WindowsEntry(targets)
            : ("bash", [Path.Combine(Repository, "build.sh"), .. targets]);

    private static (string, string[]) WindowsEntry(params string[] targets) =>
        ("cmd.exe", ["/c", Path.Combine(Repository, "build.cmd"), .. targets]);

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private static string GlobalPackagesFolder() =>
        Environment.GetEnvironmentVariable("NUGET_PACKAGES")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");

    private static string UnitTestSource(string name, string tier, bool passes) => $$"""
        public sealed class {{tier}}Tests
        {
            [Xunit.Fact]
            public void {{name}}()
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(System.Environment.GetEnvironmentVariable("FIXTURE_MARKERS")!, "{{tier}}"), "");
                Xunit.Assert.True({{(passes ? "true" : "false")}});
            }
        }
        """;

    [GeneratedRegex(@"\x1b\[[0-9;]*m")]
    private static partial Regex AnsiEscape();

    [GeneratedRegex(@"^(\w+)\s+(Succeeded|Failed|NotRun|Skipped)\b", RegexOptions.Multiline)]
    private static partial Regex SummaryLine();

    [GeneratedRegex(@"^(\S+)\s+(\d+\.\d+\.\d+\S*)\s+\S+\s+\S+", RegexOptions.Multiline)]
    private static partial Regex ToolLine();

    private void WriteTree(params (string Name, string Project)[] projects)
    {
        Directory.CreateDirectory(Markers);
        File.Copy(Path.Combine(Repository, "global.json"), Path.Combine(_root, "global.json"));
        File.Copy(Path.Combine(Repository, "Directory.Packages.props"), Path.Combine(_root, "Directory.Packages.props"));
        Write(".nuke/parameters.json", """{ "Solution": "fixture.slnx" }""");

        var entries = projects.Select(static project => $"""  <Project Path="{project.Name}/{project.Name}.csproj" />""");
        Write("fixture.slnx", $"<Solution>\n{string.Join("\n", entries)}\n</Solution>\n");

        foreach (var (name, project) in projects)
        {
            Write($"{name}/{name}.csproj", project);
        }
    }

    private void Write(string path, string content)
    {
        var full = Path.Combine(_root, path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
    }

    private async Task RunAsync((string FileName, string[] Arguments) command)
    {
        var start = new ProcessStartInfo(command.FileName)
        {
            WorkingDirectory = _root,

            // A build under test never reads the terminal. NUKE waits for a key after regenerating CI
            // configuration, which a synthetic tree always triggers, unless its input is redirected.
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["FIXTURE_MARKERS"] = Markers, ["NO_COLOR"] = "1" },
        };

        foreach (var argument in command.Arguments.Concat(command.FileName == "dotnet" ? [] : ["--root", _root, "--no-logo"]))
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        process.StandardInput.Close();
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        _output = AnsiEscape().Replace(await output + await error, string.Empty);
        _exitCode = process.ExitCode;
    }

    /// <summary>The target summary NUKE prints last, as "Target Status" lines in execution order.</summary>
    private string[] Summary() =>
        SummaryLine().Matches(_output).Select(static match => $"{match.Groups[1].Value} {match.Groups[2].Value}").ToArray();

    private string[] Packages() =>
        Directory.GetFiles(Path.Combine(_root, ".artifacts", "nupkg")).Select(static file => Path.GetFileName(file)!).ToArray();

    private Dictionary<string, string> ManifestTools()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Repository, ".config", "dotnet-tools.json")));
        return manifest.RootElement.GetProperty("tools").EnumerateObject()
            .ToDictionary(static tool => tool.Name, static tool => tool.Value.GetProperty("version").GetString()!);
    }

    private Dictionary<string, string> ListedTools() =>
        ToolLine().Matches(_output).ToDictionary(static match => match.Groups[1].Value, static match => match.Groups[2].Value);

    private const string ClassLibrary = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
          </PropertyGroup>
        </Project>
        """;

    private const string TestProject = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <OutputType>Exe</OutputType>
          </PropertyGroup>
          <ItemGroup>
            <PackageReference Include="xunit.v3" />
          </ItemGroup>
        </Project>
        """;

    private const string ToolProject = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <TargetFramework>net10.0</TargetFramework>
            <OutputType>Exe</OutputType>
            <PackAsTool>true</PackAsTool>
            <ToolCommandName>specht</ToolCommandName>
            <PackageId>specht.tool</PackageId>
          </PropertyGroup>
        </Project>
        """;

    private readonly string _root = Directory.CreateTempSubdirectory("specht-build-").FullName;

    private string _output = string.Empty;

    private int _exitCode;
}
