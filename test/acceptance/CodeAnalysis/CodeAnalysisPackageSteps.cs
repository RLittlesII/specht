using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.CodeAnalysis;

[Binding]
[Scope(Feature = "Consumer analysis adoption")]
public sealed class CodeAnalysisPackageSteps(PackedTool tool)
{
    [Given("a consumer installing Specht.CodeAnalysis through a private PackageReference")]
    public void GivenPrivatePackageReference() => _privateReference = true;

    [When("the package is consumed")]
    public async Task WhenPackageIsConsumed()
    {
        _privateReference.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
    }

    [Then("analyzers, code fixes and build integration are available")]
    public void ThenPackageAssetsAreAvailable()
    {
        using var archive = OpenAnalysisPackage();
        var entries = archive.Entries.Select(static entry => entry.FullName).ToArray();
        entries.Should().Contain("analyzers/dotnet/cs/Specht.Analysis.dll");
        entries.Should().Contain("analyzers/dotnet/cs/Specht.Codefixes.dll");
        entries.Should().Contain("build/Specht.CodeAnalysis.props");
        entries.Should().Contain("build/Specht.CodeAnalysis.targets");
    }

    [Then("no application runtime dependency is introduced")]
    public void ThenNoApplicationRuntimeDependency()
    {
        using var archive = OpenAnalysisPackage();
        var metadata = Metadata(archive);
        metadata.Descendants().Where(static element => element.Name.LocalName == "dependency").Should().BeEmpty();
        archive.Entries.Should().NotContain(static entry => entry.FullName.StartsWith("lib/", StringComparison.Ordinal));
    }

    [Given("a repository release")]
    public void GivenRepositoryRelease() => _repositoryRelease = true;

    [When("analysis and tool packages are produced")]
    public async Task WhenBothPackagesAreProduced()
    {
        _repositoryRelease.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
        _toolPackage = await tool.PackageAsync();
    }

    [Then("both use the repository release version")]
    public void ThenPackageVersionsMatch()
    {
        VersionOf(_analysisPackage!, "Specht.CodeAnalysis.").Should().Be(VersionOf(_toolPackage!, "specht.tool."));
    }

    [Given("different installed package versions with compatible schema and evidence contracts")]
    public void GivenCompatibleContracts() => _compatibleContracts = true;

    [When("interoperability is evaluated")]
    public async Task WhenInteroperabilityIsEvaluated()
    {
        _compatibleContracts.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
    }

    [Then("exact package-version equality is not required")]
    public void ThenPackageVersionEqualityIsNotRequired()
    {
        using var archive = OpenAnalysisPackage();
        Metadata(archive).Descendants().Where(static element => element.Name.LocalName == "dependency").Should().BeEmpty();
        archive.Entries.Select(static entry => entry.FullName).Should().Contain("analyzers/dotnet/cs/JsonSchema.Net.dll");
    }

    [Given(@"MSBuild root\/input configuration and standard Roslyn reporting configuration")]
    public void GivenRootAndReportingConfiguration() => _reportingConfiguration = true;

    [When("analysis is configured")]
    public async Task WhenAnalysisIsConfigured()
    {
        _reportingConfiguration.Should().BeTrue();
        _analysisPackage = await tool.CodeAnalysisPackageAsync();
    }

    [Then("MSBuild determines the repository root and supplied inputs")]
    public void ThenMsBuildSuppliesInputs()
    {
        using var archive = OpenAnalysisPackage();
        using var reader = new StreamReader(archive.GetEntry("build/Specht.CodeAnalysis.props")!.Open());
        var props = reader.ReadToEnd();
        props.Should().Contain("SpechtRoot");
        props.Should().Contain("<AdditionalFiles Include=");
        props.Should().Contain("CompilerVisibleItemMetadata");
    }

    [Then("severity and suppression use standard Roslyn controls")]
    public void ThenSeverityAndSuppressionAreStandard()
    {
        using var archive = OpenAnalysisPackage();
        using var reader = new StreamReader(archive.GetEntry("README.md")!.Open());
        reader.ReadToEnd().Should().Contain("standard Roslyn configuration");
    }

    private static XElement Metadata(ZipArchive archive)
    {
        var manifest = archive.Entries.Single(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal));
        using var stream = manifest.Open();
        return XDocument.Load(stream).Descendants().Single(static element => element.Name.LocalName == "metadata");
    }

    private static string VersionOf(string path, string prefix) => Path.GetFileNameWithoutExtension(path)[prefix.Length..];

    private ZipArchive OpenAnalysisPackage() =>
        ZipFile.OpenRead(_analysisPackage ?? throw new InvalidOperationException("No analysis package was produced."));

    private bool _privateReference;
    private bool _repositoryRelease;
    private bool _compatibleContracts;
    private bool _reportingConfiguration;
    private string? _analysisPackage;
    private string? _toolPackage;
}
