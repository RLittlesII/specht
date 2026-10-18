using AwesomeAssertions;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[Trait("Tier", "Unit")]
public sealed class BindingFeatureReaderUnitTests
{
    [Fact]
    public void ConfiguredClaimsSection_WhenRead_ShouldKeepFirstTableDuplicateIdsAndSourceLines()
    {
        // Given
        const string text = """
            ---
            epic: 0400
            id: 'F1'
            spec_status: draft
            ---

            ## Other

            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-999 | Other | Source | Active |

            ## Custom obligations

            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | `B-001` | A **visible** claim | Source | Active |
            | B-001 | Another declaration | Source | Superseded |

            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-002 | Second table | Source | Active |
            """;
        BindingPolicy policy = new BindingPolicyFixture().WithClaimsSection("Custom obligations");
        var reader = new BindingFeatureReader();

        // When
        var feature = reader.Read(text, "src/example/.spec/README.md", policy);

        // Then
        feature.Identity.Epic.Should().Be("0400");
        feature.Identity.Feature.Should().Be("F1");
        feature.State.Should().Be("draft");
        feature.RelativePath.Should().Be("src/example/.spec/README.md");
        feature.Claims.Select(static claim => claim.Id).Should().Equal("B-001", "B-001");
        feature.Claims.Select(static claim => claim.State).Should().Equal("Active", "Superseded");
        feature.Claims.Select(static claim => claim.Line).Should().Equal(17, 18);
    }

    [Theory]
    [ClassData(typeof(FrontmatterIdentityCases))]
    public void FrontmatterScalars_WhenProjected_ShouldPreserveStringsWithoutCoercingOtherYamlTypes(string yaml, string? expected)
    {
        // Given
        var reader = new BindingFeatureReader();
        var text = $"---\nepic: {yaml}\nid: F1\nspec_status: draft\n---\n";

        // When
        var feature = reader.Read(text, "src/example/.spec/README.md", new BindingPolicyFixture());

        // Then
        feature.Identity.Epic.Should().Be(expected);
    }

    [Fact]
    public void IdenticalInputsWithoutFrameworkServices_WhenResolvedTwice_ShouldYieldTheSameIdentityAndFindings()
    {
        // Given
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new BindingFeature[] { new BindingFeatureFixture() });
        var resolver = new BindingResolver(catalog, new BindingPolicyFixture());
        ClaimReference reference = new ClaimReferenceFixture();

        // When
        var first = resolver.Resolve(reference);
        var second = resolver.Resolve(reference);

        // Then
        first.Status.Should().Be(BindingResolution.Resolved);
        first.Target!.Id.Should().Be("B-001");
        first.IsEligible.Should().BeTrue();
        second.Should().BeEquivalentTo(first, static options => options.WithStrictOrdering());
    }
}
