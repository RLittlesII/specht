using AwesomeAssertions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Unit")]
public sealed class BindingProvenanceUnitTests
{
    [Theory]
    [ClassData(typeof(UnsafeProvenancePathCases))]
    public void UnsafeProvenanceAtEachInputBoundary_WhenPrepared_ShouldRejectWithFixedPathFreeError(string path)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture();
        BindingAdr original = new BindingAdrFixture();
        const string text = "---\nepic: 0400\nid: F1\nspec_status: draft\n---\n\n## 3. Acceptance Criteria\n\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | A need | Source | Active |\n";
        BindingFeature? textFeature = null;
        BindingResult? featureResult = null;
        BindingResult? adrResult = null;

        // When
        Action prepareText = () => textFeature = new BindingFeatureReader().Read(text, path, policy);
        Action prepareFeature = () =>
        {
            BindingFeature feature = new BindingFeatureFixture().WithRelativePath(path);
            BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new[] { feature });
            featureResult = new BindingResolver(catalog, policy).Resolve(new ClaimReferenceFixture());
        };
        Action prepareAdr = () =>
        {
            var changed = original with { RelativePath = path };
            BindingCatalog catalog = new BindingCatalogFixture().WithAdrs(new[] { changed });
            adrResult = new BindingResolver(catalog, policy).Resolve(new AdrReferenceFixture());
        };

        // Then
        prepareText.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(FixedError);
        prepareFeature.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(FixedError);
        prepareAdr.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(FixedError);
        textFeature.Should().BeNull();
        featureResult.Should().BeNull();
        adrResult.Should().BeNull();
        original.RelativePath.Should().Be(".spec/adr/0001-choice.md");
    }

    [Theory]
    [ClassData(typeof(ContainedProvenancePathCases))]
    public void ContainedProvenanceAtEachInputBoundary_WhenResolved_ShouldRetainTheSuppliedSpelling(string path)
    {
        // Given
        BindingPolicy policy = new BindingPolicyFixture();
        BindingAdr original = new BindingAdrFixture();
        const string text = "---\nepic: 0400\nid: F1\nspec_status: draft\n---\n\n## 3. Acceptance Criteria\n\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | A need | Source | Active |\n";
        var changed = original with { RelativePath = path };

        // When
        var textFeature = new BindingFeatureReader().Read(text, path, policy);
        BindingCatalog textCatalog = new BindingCatalogFixture().WithFeatures(new[] { textFeature });
        var textResult = new BindingResolver(textCatalog, policy).Resolve(new ClaimReferenceFixture());
        BindingFeature feature = new BindingFeatureFixture().WithRelativePath(path);
        BindingCatalog featureCatalog = new BindingCatalogFixture().WithFeatures(new[] { feature });
        var featureResult = new BindingResolver(featureCatalog, policy).Resolve(new ClaimReferenceFixture());
        BindingCatalog adrCatalog = new BindingCatalogFixture().WithAdrs(new[] { changed });
        var adrResult = new BindingResolver(adrCatalog, policy).Resolve(new AdrReferenceFixture());

        // Then
        textResult.Status.Should().Be(BindingResolution.Resolved);
        featureResult.Status.Should().Be(BindingResolution.Resolved);
        adrResult.Status.Should().Be(BindingResolution.Resolved);
        textResult.Target!.RelativePath.Should().Be(path);
        featureResult.Target!.RelativePath.Should().Be(path);
        adrResult.Target!.RelativePath.Should().Be(path);
        original.RelativePath.Should().Be(".spec/adr/0001-choice.md");
    }

    private const string FixedError = "Binding provenance must be repository-relative and remain within the root.";
}
