using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Model;

/// <summary>Shared claim and graph derivations preserve source order, duplicates and configured comparisons.</summary>
[Trait("Tier", "Unit")]
public sealed class SpecModelDerivationsUnitTests
{
    [Fact]
    public void AnInvalidLegacyClaimGrammarWithItsRulesDisabled_WhenTheModelIsEvaluated_ShouldNotRunOrValidateThoseRules()
    {
        // Given
        SpecModel model = new SpecModelFixture().WithManifest(new JsonObject
        {
            ["identifiers"] = new JsonObject { ["claim"] = "[" },
            ["rules"] = new JsonObject { ["SPEC020"] = "off", ["SPEC021"] = "off", ["SPEC030"] = "off", ["SPEC031"] = "off" },
        });

        // When
        var report = SpechtRunner.Evaluate(model, [new Specht.Rules.FeatureFileRule(), new Specht.Rules.ClaimRule()]);

        // Then
        report.RulesEvaluated.Should().Be(0);
        report.Violations.Should().BeEmpty();
    }

    [Fact]
    public void ClaimRowsUnderRenamedRoles_WhenTheModelIsBuilt_ShouldKeepMalformedAndDuplicateRowsAndTheConfiguredMatrix()
    {
        // Given
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
            "## 3. Claims\n\n| ID | Claim |\n| -- | ----- |\n| B-001 | First. |\n| B-001 | Duplicate. |\n| B-1 | Invalid. |\n"
            + "\n## 9. Coverage\n\n| ID | Test |\n| -- | ---- |\n| B-001 | Missing |\n", "src/sample/.spec/README.md"));

        // When
        SpecModel model = new SpecModelFixture().WithFeatures(feature).WithManifest(new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["sections"] = new JsonArray("3. Claims", "9. Coverage", "12. Sign-off"),
            ["roles"] = new JsonObject { ["claims"] = "3. Claims", ["matrix"] = "9. Coverage" },
            ["claim"] = new JsonObject { ["tagIgnoreCase"] = true },
        });

        // Then
        var claims = model.Claims[feature];
        claims.Rows.Select(static claim => (claim.Id, claim.Line, claim.IsValid)).Should().Equal(
            ("B-001", 5, true), ("B-001", 6, true), ("B-1", 7, false));
        claims.Rows.Select(static claim => claim.FirstDeclarationLine).Should().Equal(new int?[] { null, 5, null });
        claims.Ids.Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, int>("B-001", 5));
        claims.TagIds.Contains("b-001").Should().BeTrue();
        model.MatrixSections[feature]!.Title.Should().Be("9. Coverage");
    }

    [Fact]
    public void DuplicateIdentitiesAndMixedEdges_WhenTheModelIsBuilt_ShouldKeepTheLastIdentitysGraphAndResolveBothEdgeForms()
    {
        // Given
        FeatureSpec first = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
            "---\nepic: '0001'\nid: F1\ndepends_on: ['F9']\n---\n", "src/first/.spec/README.md"));
        FeatureSpec last = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
            "---\nepic: '0001'\nid: F1\ndepends_on: ['F2', '0002/F3', 'F2']\nblocks: ['F4']\n---\n", "src/last/.spec/README.md"));
        FeatureSpec unidentified = new FeatureSpecFixture();

        // When
        SpecModel model = new SpecModelFixture().WithFeatures(first, last, unidentified);

        // Then
        model.FeaturesWithIdentity.Should().Equal(first, last);
        model.DependencyGraph.Identities.Should().Equal("0001-F1");
        model.DependencyGraph.DependsOn["0001-F1"].Should().Equal("0001-F2", "0002-F3");
        model.DependencyGraph.Blocks["0001-F1"].Should().Equal("0001-F4");
    }
}
