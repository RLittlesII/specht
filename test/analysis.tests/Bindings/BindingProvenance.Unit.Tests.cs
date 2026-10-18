using AwesomeAssertions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Unit")]
public sealed class BindingProvenanceUnitTests
{
    [Theory]
    [ClassData(typeof(ProvenancePathCases))]
    public void ProvenanceAtEachInputBoundary_WhenPrepared_ShouldRejectUnsafePathsAndRetainContainedPaths(string category, bool allowed)
    {
        // Given
        var path = category switch
        {
            "POSIX root" => '/' + "outside.md",
            "Windows drive" => "Q" + ':' + '\\' + "outside.md",
            "Windows drive relative" => "Q" + ':' + "outside.md",
            "UNC root" => new string('\\', 2) + "server" + '\\' + "outside.md",
            "backslash root" => '\\' + "outside.md",
            "forward escape" => string.Join('/', "safe", "..", "..", "outside.md"),
            "backslash escape" => string.Join('\\', "safe", "..", "..", "outside.md"),
            "mixed escape" => "safe" + '\\' + ".." + '/' + ".." + '\\' + "outside.md",
            "nested relative" => "src/one/.spec/README.md",
            "backslash relative" => string.Join('\\', "src", "one", ".spec", "README.md"),
            "contained parent" => "src/old/../one/.spec/README.md",
            _ => throw new InvalidOperationException("Unknown provenance category."),
        };
        BindingPolicy policy = new BindingPolicyFixture();
        BindingAdr original = new BindingAdrFixture();
        const string text = "---\nepic: 0400\nid: F1\nspec_status: draft\n---\n\n## 3. Acceptance Criteria\n\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | A need | Source | Active |\n";
        BindingFeature? textFeature = null;
        BindingResult? textResult = null;
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
        if (allowed)
        {
            prepareText.Should().NotThrow();
            prepareFeature.Should().NotThrow();
            prepareAdr.Should().NotThrow();
            BindingCatalog textCatalog = new BindingCatalogFixture().WithFeatures(new[] { textFeature! });
            textResult = new BindingResolver(textCatalog, policy).Resolve(new ClaimReferenceFixture());
            textResult.Status.Should().Be(BindingResolution.Resolved);
            featureResult!.Status.Should().Be(BindingResolution.Resolved);
            adrResult!.Status.Should().Be(BindingResolution.Resolved);
            textResult.Target!.RelativePath.Should().Be(path);
            featureResult.Target!.RelativePath.Should().Be(path);
            adrResult.Target!.RelativePath.Should().Be(path);
        }
        else
        {
            prepareText.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(FixedError);
            prepareFeature.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(FixedError);
            prepareAdr.Should().ThrowExactly<ArgumentException>().Which.Message.Should().Be(FixedError);
            textFeature.Should().BeNull();
            textResult.Should().BeNull();
            featureResult.Should().BeNull();
            adrResult.Should().BeNull();
        }
        original.RelativePath.Should().Be(".spec/adr/0001-choice.md");
    }

    private const string FixedError = "Binding provenance must be repository-relative and remain within the root.";
}
