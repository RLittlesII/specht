using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using specht.tests;

namespace specht.acceptance.Manifest;

/// <summary>
/// Steps for <c>src/specht/Manifest/.spec/manifest.feature</c> (0001-F5). The manifest is edited in memory from the tree's
/// default copy and written when the check runs, so a tree's violations are counted under the default manifest first. A run
/// is the engine's runner; the exit code and stream of a rejection are 0013's to bind.
/// </summary>
[Binding]
[Scope(Feature = "The manifest carries the roles")]
public sealed class ManifestSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given("the manifest is the default one this repository checks itself with")]
    public void GivenTheManifestIsTheDefaultOneThisRepositoryChecksItselfWith() =>
        _manifest = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();

    [Given("the manifest carries a key the engine does not know")]
    public void GivenTheManifestCarriesAKeyTheEngineDoesNotKnow() => Manifest[UnknownKey] = "synthetic";

    [Given("the manifest carries a {string} key and a {string} key")]
    public void GivenTheManifestCarriesAKeyAndAKey(string first, string second)
    {
        Manifest[first] = "synthetic";
        Manifest[second] = "synthetic";
    }

    [Given("the manifest declares no claim grammar")]
    public void GivenTheManifestDeclaresNoClaimGrammar()
    {
        var identifiers = Manifest["identifiers"]!.AsObject();
        _defaultClaimGrammar = identifiers["claim"]!.GetValue<string>();
        identifiers.Remove("claim");
    }

    [Given("the root holds a specification declaring claim {string}")]
    public void GivenTheRootHoldsASpecificationDeclaringClaim(string claim) =>
        Tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + $"| {claim} | It does the thing. | brd | Active |\n"));

    [Then("the malformed-claim-id rule reports {string} against the default claim grammar")]
    public void ThenTheMalformedClaimIdRuleReportsAgainstTheDefaultClaimGrammar(string claim) =>
        _report!.Violations
            .Should()
            .ContainSingle(violation => violation.RuleId == "SPEC030" && violation.Identifier == claim)
            .Which.Message.Should()
            .Contain(_defaultClaimGrammar ?? throw new InvalidOperationException("No claim grammar was removed."));

    [Given("the root holds a specification with three violations")]
    public void GivenTheRootHoldsASpecificationWithThreeViolations()
    {
        var sections = SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims)
            .Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal))
            .ToList();
        Tree.WriteFeatureFile(Tree.WriteFeature("0001", "F1", sections: sections), PhantomTag);
        Tree.Run().Violations.Should().HaveCount(3);
    }

    [Given("the root holds a specification with no violations")]
    public void GivenTheRootHoldsASpecificationWithNoViolations()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.Run().Violations.Should().BeEmpty();
    }

    [When("the check runs")]
    public void WhenTheCheckRuns()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());

        try
        {
            _report = SpecCheckRunner.Run(Tree.Root);
        }
        catch (SpecManifestException rejection)
        {
            _rejection = rejection;
        }
    }

    [Then("the manifest is rejected")]
    public void ThenTheManifestIsRejected() => _rejection.Should().NotBeNull();

    [Then("the rejection names that key")]
    public void ThenTheRejectionNamesThatKey() => _rejection!.Message.Should().Contain(UnknownKey);

    [Then("none of the three violations is reported")]
    public void ThenNoneOfTheThreeViolationsIsReported() => _report.Should().BeNull();

    [Then("the manifest is accepted")]
    public void ThenTheManifestIsAccepted()
    {
        _rejection.Should().BeNull();
        _report.Should().NotBeNull();
    }

    [Then("no violation is reported")]
    public void ThenNoViolationIsReported() => _report!.Violations.Should().BeEmpty();

    [AfterScenario]
    public void DeleteTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private JsonObject Manifest => _manifest ?? throw new InvalidOperationException("No manifest was read.");

    private string ManifestPath => Path.Combine(Tree.Root, ".spec", "schema", "spec-structure.schema.json");

    private const string UnknownKey = "glossary";

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private const string PhantomTag =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n";

    private SpecTree? _tree;
    private JsonObject? _manifest;
    private SpecCheckReport? _report;
    private SpecManifestException? _rejection;
    private string? _defaultClaimGrammar;
}
