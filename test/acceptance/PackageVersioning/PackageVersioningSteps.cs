using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.PackageVersioning;

/// <summary>
/// Steps for <c>.build/Versioning/.spec/versioning.feature</c> (0055-F5): 0080 binds B-005 and nothing else, by the owner's
/// call (decision 0002). The commit is the one this repository has checked out. "The tool is packed" issues the command the
/// build's <c>Pack</c> target issues - <c>dotnet pack</c> on the solution, already compiled, in the configuration this
/// assembly was built in - into a temporary directory instead of the build's package output, so it never runs the build
/// from inside the build's own test target. The version is asked of the <c>nbgv</c> tool on the same commit and ref, never
/// written here. The file's other scenarios stay pending for their items.
/// </summary>
[Binding]
[Scope(Feature = "Package versioning")]
public sealed class PackageVersioningSteps
{
    [Given("a commit")]
    public async Task GivenACommit()
    {
        var (stdout, stderr, exitCode) = await DotNetAsync("nbgv", "get-version", "--variable", "NuGetPackageVersion");
        exitCode.Should().Be(0, stdout + stderr);
        _computed = stdout.Trim();
    }

    [When("the tool is packed")]
    public async Task WhenTheToolIsPacked()
    {
        _packages = Directory.CreateTempSubdirectory("specht-pack-").FullName;
        var (stdout, stderr, exitCode) = await DotNetAsync(
            "pack", "specht.slnx", "--configuration", Configuration, "--no-build", "--output", _packages);
        exitCode.Should().Be(0, stdout + stderr);
    }

    [Then("the package's version is the version computed for that commit")]
    public void ThenThePackagesVersionIsTheVersionComputedForThatCommit()
    {
        var package = Directory.GetFiles(Packages).Should().ContainSingle().Subject;
        Path.GetFileName(package).Should().Be($"tool.{Computed}.nupkg");

        using var archive = ZipFile.OpenRead(package);
        using var nuspec = archive.GetEntry("tool.nuspec")!.Open();
        XDocument.Load(nuspec).Descendants().Single(static element => element.Name.LocalName == "version").Value.Should().Be(Computed);
    }

    [AfterScenario]
    public void DeletePackages()
    {
        if (_packages is not null)
        {
            Directory.Delete(_packages, recursive: true);
        }
    }

    private static string Repository { get; } = FindRepository(AppContext.BaseDirectory);

    private static string Configuration { get; } =
        typeof(PackageVersioningSteps).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
        ?? throw new InvalidOperationException("The test assembly records no build configuration.");

    private string Computed => _computed ?? throw new InvalidOperationException("No version was computed.");

    private string Packages => _packages ?? throw new InvalidOperationException("The tool was not packed.");

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                             ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private static async Task<(string Stdout, string Stderr, int ExitCode)> DotNetAsync(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Repository,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["MSBUILDDISABLENODEREUSE"] = "1", ["NO_COLOR"] = "1" },
        };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (await stdout, await stderr, process.ExitCode);
    }

    private string? _computed;
    private string? _packages;
}
