using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.PackageVersioning;

/// <summary>
/// Steps for <c>.build/Versioning/.spec/versioning.feature</c> (0055-F5): 0080 binds B-005 and nothing else, by the owner's
/// call (decision 0002). The commit is the one this repository has checked out. "The tool is packed" is
/// <see cref="PackedToolSteps"/>'s, shared with the check's scenarios. The version is asked of the <c>nbgv</c> tool on the
/// same commit and ref, never written here. The package's id is the one 0001-F2 B-017 claims, never read back from the
/// project that packs. B-006 is bound by SchemaVersionIndependenceSteps; the remaining scenarios stay pending.
/// </summary>
[Binding]
[Scope(Feature = "Package versioning")]
public sealed class PackageVersioningSteps(PackedTool tool)
{
    [Given("a commit")]
    public async Task GivenACommit()
    {
        var (stdout, stderr, exitCode) = await tool.DotNetAsync("nbgv", "get-version", "--variable", "NuGetPackageVersion");
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

    private string Computed => _computed ?? throw new InvalidOperationException("No version was computed.");

    private string? _computed;
}
