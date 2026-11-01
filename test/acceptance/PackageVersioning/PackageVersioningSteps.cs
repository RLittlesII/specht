using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.PackageVersioning;

/// <summary>
/// Steps for <c>.build/Versioning/.spec/versioning.feature</c> (0055-F5). B-005 is bound against the commit this
/// repository has checked out, by the owner's call (decision 0002); "the tool is packed" is <see cref="PackedToolSteps"/>'s.
/// B-003 and B-009 compute the version of a synthetic one-commit repository on <c>main</c> that holds this repository's
/// <c>version.json</c> as it stands, outside any runner. The version is always asked of the <c>nbgv</c> tool, never written
/// here. B-006 is bound by SchemaVersionIndependenceSteps; the remaining scenarios stay pending.
/// </summary>
[Binding]
[Scope(Feature = "Package versioning")]
public sealed class PackageVersioningSteps(PackedTool tool) : IDisposable
{
    public void Dispose()
    {
        if (_synthetic is not null)
        {
            Directory.Delete(_synthetic, recursive: true);
        }
    }

    [Given("a commit")]
    public async Task GivenACommit()
    {
        var (stdout, stderr, exitCode) = await tool.DotNetAsync("nbgv", "get-version", "--variable", "NuGetPackageVersion");
        exitCode.Should().Be(0, stdout + stderr);
        _computed = stdout.Trim();
    }

    [Given("a commit on the main branch")]
    public Task GivenACommitOnTheMainBranch() => CommitTheVersionLineAsync();

    [Given("a commit whose version line is below 1.0.0")]
    public async Task GivenACommitWhoseVersionLineIsBelowOne()
    {
        JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(tool.Repository, "version.json")))!["version"]!.GetValue<string>()
            .Should().StartWith("0.");
        await CommitTheVersionLineAsync();
    }

    [When("its package version is computed")]
    public async Task WhenItsPackageVersionIsComputed()
    {
        var (stdout, stderr, exitCode) = await tool.DotNetAsync(
            new Dictionary<string, string> { ["GITHUB_ACTIONS"] = string.Empty },
            "nbgv", "get-version", "-p", Synthetic, "--variable", "NuGetPackageVersion");
        exitCode.Should().Be(0, stdout + stderr);
        _computed = stdout.Trim();
    }

    [Then("the package's version is the version computed for that commit")]
    public async Task ThenThePackagesVersionIsTheVersionComputedForThatCommit()
    {
        var package = await tool.PackageAsync();
        Path.GetFileName(package).Should().Be($"specht.tool.{Computed}.nupkg");

        using var archive = ZipFile.OpenRead(package);
        using var nuspec = archive.GetEntry("specht.tool.nuspec")!.Open();
        XDocument.Load(nuspec).Descendants().Single(static element => element.Name.LocalName == "version").Value.Should().Be(Computed);
    }

    [Then("the version carries no suffix naming the commit")]
    public async Task ThenTheVersionCarriesNoSuffixNamingTheCommit() =>
        Computed.Should().NotContain((await Git("rev-parse", "HEAD")).Trim()[..7]);

    [Then("the version carries the prerelease label {string}")]
    public void ThenTheVersionCarriesThePrereleaseLabel(string label)
    {
        var parts = Computed.Split('-', 2);
        parts.Should().HaveCount(2, "a prerelease version carries a label after its number");
        parts[1].Split('-', '.')[0].Should().Be(label);
    }

    private string Computed => _computed ?? throw new InvalidOperationException("No version was computed.");

    private string Synthetic => _synthetic ?? throw new InvalidOperationException("No commit was made.");

    private async Task CommitTheVersionLineAsync()
    {
        _synthetic = Directory.CreateTempSubdirectory("specht-version-line-").FullName;
        File.Copy(Path.Combine(tool.Repository, "version.json"), Path.Combine(_synthetic, "version.json"));
        await Git("init", "--initial-branch=main");
        await Git("add", "version.json");
        await Git("-c", "user.name=Synthetic", "-c", "user.email=synthetic@example.invalid",
            "-c", "core.hooksPath=/dev/null", "-c", "commit.gpgsign=false", "commit", "-m", "Synthetic version line");
    }

    private async Task<string> Git(params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = Synthetic,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("git did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        process.ExitCode.Should().Be(0, await stderr);

        return await stdout;
    }

    private string? _computed;
    private string? _synthetic;
}
