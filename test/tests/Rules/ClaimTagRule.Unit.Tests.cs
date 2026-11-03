using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The claim-tag rule over a model built in memory under a manifest that renames the claims section (<c>0001-F5</c>
/// B-001, decision 0006), its companion’s parsed tags supplied in memory (ADR-0010 stage 2): a
/// scenario tag resolves against the section the claims role names, and the title the engine once hardcoded is a section
/// like any other. SPEC021's order and guard (<c>0001-F1</c> B-004; item 0159): the Features' order and, within one
/// companion, the tags' order in the file; a Feature without exactly one companion reports nothing, whatever SPEC020's level.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ClaimTagRuleUnitTests
{
    [Fact]
    public void ATagDeclaredOnlyUnderTheTitleNoRoleNames_WhenTheRuleEvaluatesTheModel_ShouldReportSpec021OnItAndNotOnTheTagTheClaimsRolesSectionDeclares()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Claims", "9. Traceability Matrix", "12. Sign-off"),
            ["roles"] = new JsonObject { ["claims"] = "3. Claims" },
        };
        var companion = Path.Combine("repo", "src", "sample", ".spec", "sample.feature");
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [companion] = new(
                    "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n"
                        + "  @B-404\n  Scenario: Phantom\n    Given nothing\n"),
            });
        FeatureSpec feature = new FeatureSpecFixture()
            .WithDocument(
                DocumentParser.Parse(
                    "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-404 | Under the title no role names. |\n\n"
                        + "## 3. Claims\n\n| ID | Claim |\n| -- | ----- |\n| B-001 | Under the claims role's section. |\n",
                    "src/sample/.spec/README.md"))
            .WithFeatureFiles(new FeatureFileFixture().WithTags(new FeatureTagParser().Parse(fileSystem.File.ReadAllText(companion))));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        var rule = new ClaimTagRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.Identifier)).Should().Equal(("SPEC021", "B-404"));
    }

    [Fact]
    public void CompanionsWithUnresolvedTags_WhenTheClaimTagRuleEvaluatesTheModel_ShouldReportSpec021InFeatureOrderThenTagOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", Companion("src/b/.spec/b.feature", new FeatureTag("B-003", 4), new FeatureTag("B-002", 9))),
            Feature("src/a/.spec/README.md", Companion("src/a/.spec/a.feature", new FeatureTag("B-009", 2))));
        var rule = new ClaimTagRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC021", "src/b/.spec/b.feature", 4, "B-003"),
            ("SPEC021", "src/b/.spec/b.feature", 9, "B-002"),
            ("SPEC021", "src/a/.spec/a.feature", 2, "B-009"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void FeaturesWithNoneOrTwoCompanions_WhenTheClaimTagRuleEvaluatesTheModel_ShouldReportNothingForThemWhateverTheSiblingLevel(string manifest)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(manifest)!.AsObject()).WithFeatures(
            Feature("src/none/.spec/README.md"),
            Feature(
                "src/two/.spec/README.md",
                Companion("src/two/.spec/first.feature", new FeatureTag("B-008", 2)),
                Companion("src/two/.spec/second.feature", new FeatureTag("B-009", 2))),
            Feature("src/one/.spec/README.md", Companion("src/one/.spec/one.feature", new FeatureTag("B-007", 2))));
        var rule = new ClaimTagRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(("SPEC021", "src/one/.spec/one.feature", "B-007"));
    }

    /// <summary>The manifest with SPEC020 at its default level, and with it off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC020": "off" } }""");
        }
    }

    private static SpechtDocumentParser DocumentParser => new(new FrontmatterReader());

    private static FeatureFile Companion(string path, params FeatureTag[] tags) => new FeatureFileFixture().WithRelativePath(path).WithTags(tags);

    private static FeatureSpec Feature(string path, params FeatureFile[] companions) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(
                new SpechtDocument(path, new FrontmatterFixture(), [new SpechtSection(Claims, 5, ["ID", "Claim"], [["B-001", "It holds."]], [7])]))
            .WithFeatureFiles(companions);

    private const string Claims = "3. Acceptance Criteria";
}
