using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceCatalogExporterUnitTests
{
    [Fact]
    public void NativeCatalog_WhenProjected_ShouldCarryEachFeatureClaimAndCase()
    {
        // Given
        var claim = new BindingClaim("REQ-FIX-001", "Active", 7, RelativePath: Path, Element: BindingClaimElement.ScenarioOutline,
            Cases: [new BindingClaimCase(13, false), new BindingClaimCase(14, true)]);
        var catalog = new BindingCatalog([new BindingFeature(new FeatureIdentity(null, "arithmetic"), "current", [claim], Path)], []);

        // When
        var features = new TestEvidenceCatalogExporter().Project(catalog);

        // Then
        features.Should().BeEquivalentTo([new TestEvidenceCatalogFeature(null, "arithmetic", "current", Path,
            [new TestEvidenceCatalogClaim("REQ-FIX-001", "Active", "scenarioOutline", Path, 7,
                [new TestEvidenceCatalogCase(13, false), new TestEvidenceCatalogCase(14, true)])])], static options => options.WithStrictOrdering());
    }

    [Theory]
    [InlineData(BindingClaimElement.Feature, "feature")]
    [InlineData(BindingClaimElement.Rule, "rule")]
    [InlineData(BindingClaimElement.Scenario, "scenario")]
    [InlineData(BindingClaimElement.ScenarioOutline, "scenarioOutline")]
    public void ClaimElement_WhenProjected_ShouldBeWrittenInTheContractSpelling(BindingClaimElement element, string spelling)
    {
        // Given
        var claim = new BindingClaim("REQ-FIX-001", "Active", 3, RelativePath: Path, Element: element, Cases: []);
        var catalog = new BindingCatalog([new BindingFeature(new FeatureIdentity(null, "arithmetic"), "current", [claim], Path)], []);

        // When
        var features = new TestEvidenceCatalogExporter().Project(catalog);

        // Then
        features.Should().ContainSingle().Which.Claims.Should().ContainSingle().Which.Element.Should().Be(spelling);
    }

    [Fact]
    public void FeaturesInAnyOrder_WhenProjected_ShouldBeSortedByEpicNullFirstThenFeatureOrdinally()
    {
        // Given
        BindingFeature Feature(string? epic, string name) => new(new FeatureIdentity(epic, name), "current",
            [new BindingClaim("REQ-FIX-001", "Active", 1, RelativePath: Path, Element: BindingClaimElement.Scenario, Cases: [])], Path);
        var catalog = new BindingCatalog([Feature("0201", "alpha"), Feature(null, "zeta"), Feature(null, "beta")], []);

        // When
        var features = new TestEvidenceCatalogExporter().Project(catalog);

        // Then
        features.Select(static feature => (feature.Epic, feature.Feature)).Should()
            .Equal((null, "beta"), (null, "zeta"), ("0201", "alpha"));
    }

    [Fact]
    public void ClaimsInDocumentOrder_WhenProjected_ShouldKeepThatOrder()
    {
        // Given
        BindingClaim Claim(string id, int line) => new(id, "Active", line, RelativePath: Path, Element: BindingClaimElement.Scenario, Cases: []);
        var catalog = new BindingCatalog([new BindingFeature(new FeatureIdentity(null, "arithmetic"), "current",
            [Claim("REQ-FIX-009", 4), Claim("REQ-FIX-001", 9)], Path)], []);

        // When
        var features = new TestEvidenceCatalogExporter().Project(catalog);

        // Then
        features.Single().Claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-009", "REQ-FIX-001");
    }

    private const string Path = ".spec/features/arithmetic/arithmetic.feature";
}
