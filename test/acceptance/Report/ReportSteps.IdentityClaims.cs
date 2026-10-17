using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Report;

public sealed partial class ReportSteps
{
    [Given("the root holds a specification in the epics layout whose epic directory does not match its frontmatter")]
    public void GivenAnEpicDirectoryDisagreesWithItsFrontmatter() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["epic"] = "\"0009\"" });

    [Then("the violation's expectation carries the identity form")]
    public void ThenTheIdentityExpectationCarriesItsForm() =>
        IdentityExpectation["form"]!.GetValue<string>().Should().Be(Manifest["identifiers"]!["epic"]!.GetValue<string>(), Json);

    [Then("the identity the directory implies")]
    public void ThenTheIdentityExpectationCarriesTheDirectorysValue() =>
        IdentityExpectation["value"]!.GetValue<string>().Should().Be("0001", Json);

    [Given("the root holds the same Feature identity in both layouts")]
    public void GivenTheSameIdentityIsInBothLayouts()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.WriteCoLocatedFeature("src/duplicate", "0001", "F1");
    }

    [Then("each reused identity expectation carries its form, value and first declaration")]
    public void ThenReusedIdentitiesNameTheirFirstDeclaration()
    {
        var expected = new JsonObject
        {
            ["form"] = "{epic}-{feature}",
            ["value"] = "0001-F1",
            ["firstDeclaration"] = "epics/0001-epic/F1-feature/spec.md",
        };
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC012").Should().HaveCount(2)
            .And.AllSatisfy(violation => JsonNode.DeepEquals(violation["expected"], expected).Should().BeTrue(Json));
    }

    [Given(@"^the root holds an item whose (id|parent) disagrees with its (file name|Feature)$")]
    public void GivenAnItemIdentityDisagreesWithItsSource(string key, string source)
    {
        var path = Tree.WriteFeature("0001", "F1");
        Tree.WriteItem(path, "0001-01", key == "parent" ? "0009-F9" : "0001-F1", idOverride: key == "id" ? "0001-02" : null);
    }

    [Then("the item expectation carries its {word} form and the {word} implied")]
    public void ThenTheItemExpectationCarriesItsFormAndValue(string form, string value)
    {
        var expected = Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC043")
            .Should().ContainSingle(Json).Which["expected"]!;
        expected["form"]!.GetValue<string>().Should().Be(
            form == "task" ? Manifest["identifiers"]!["task"]!.GetValue<string>() : "{epic}-{feature}", Json);
        expected["value"]!.GetValue<string>().Should().Be(value, Json);
    }

    [Given("the root holds a specification whose companion carries a tag for a claim it does not declare")]
    public void GivenTheCompanionNamesAnUndeclaredClaim()
    {
        var path = Tree.WriteFeature("0001", "F1");
        Tree.WriteFeatureFile(path, "Feature: Sample\n\n  @B-404\n  Scenario: Unknown\n    Given a thing\n");
    }

    [Then("the violation's expectation carries the claim ids the specification declares")]
    public void ThenTheCompanionExpectationCarriesDeclaredClaims() =>
        JsonNode.DeepEquals(Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC021")
            .Should().ContainSingle(Json).Which["expected"]!["claims"], new JsonArray("B-001")).Should().BeTrue(Json);

    [Given("the root holds a specification with {int} companions")]
    public void GivenTheSpecificationHasACompanionCount(int count)
    {
        Tree.WriteFeature("0001", "F1", featureFile: count == 0 ? null : "feature");
        if (count == 2)
        {
            Tree.WriteRaw("epics/0001-epic/F1-feature/second.feature", "Feature: Second\n");
        }
    }

    [Then("the companion expectation requires exactly one file")]
    public void ThenTheCompanionExpectationRequiresOneFile() =>
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC020")
            .Should().ContainSingle(Json).Which["expected"]!["companionCount"]!.GetValue<int>().Should().Be(1, Json);

    [Given("the root holds a specification declaring a claim as {string}")]
    public void GivenTheSpecificationDeclaresAMalformedClaim(string claim) =>
        Tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims.Replace("B-002", claim)));

    [Then("the violation's expectation carries the claim grammar")]
    public void ThenTheClaimExpectationCarriesItsGrammar() =>
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC030")
            .Should().ContainSingle(Json).Which["expected"]!["grammar"]!.GetValue<string>()
            .Should().Be(Manifest["identifiers"]!["claim"]!.GetValue<string>(), Json);

    [Given("the root holds a specification declaring B-001 twice")]
    public void GivenTheSpecificationDeclaresAClaimTwice()
    {
        var path = Tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria",
            TwoClaims + "| B-001 | Duplicate. | brd | Active |\n"));
        _firstClaimLine = SpecDocument.Parse(File.ReadAllText(path), "epics/0001-epic/F1-feature/spec.md")
            .Section("3. Acceptance Criteria")!.RowLines[0];
    }

    [Then("the duplicate claim expectation carries its grammar and first declaration line")]
    public void ThenTheDuplicateClaimNamesItsFirstLine()
    {
        ThenTheClaimExpectationCarriesItsGrammar();
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC030")
            .Should().ContainSingle(Json).Which["expected"]!["firstDeclarationLine"]!.GetValue<int>().Should().Be(_firstClaimLine, Json);
    }

    [Given("the root holds a specification whose claim B-003 has two traceability rows")]
    public void GivenClaimB003HasTwoMatrixRows() => GivenTheMatrixHasADefect("duplicate");

    [Given("the root holds a specification with {word} matrix rows")]
    public void GivenTheMatrixHasADefect(string defect)
    {
        var rows = "| B-001 | One | Test | Covered |\n| B-002 | Two | Test | Covered |\n";
        if (defect != "missing")
        {
            rows += "| B-003 | Three | Test | Covered |\n";
        }

        rows += defect switch
        {
            "duplicate" => "| B-003 | Again | Test | Covered |\n",
            "undeclared" => "| B-404 | Unknown | Test | Covered |\n",
            _ => string.Empty,
        };
        var sections = SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims + "| B-003 | Three. | brd | Active |\n").ToList();
        sections[8] = "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n" + rows;
        Tree.WriteFeature("0001", "F1", sections: sections);
        _matrixCounts = new JsonObject { ["B-001"] = 1, ["B-002"] = 1, ["B-003"] = defect == "missing" ? 0 : defect == "duplicate" ? 2 : 1 };
    }

    [Then("the violation's expectation names B-003 as needing exactly one row")]
    public void ThenTheMatrixExpectationRequiresOneRowForB003()
    {
        var expected = Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC031")
            .Should().ContainSingle(Json).Which["expected"]!;
        expected["requiredRows"]!.GetValue<int>().Should().Be(1, Json);
        expected["rowsByClaim"]!.AsObject().Should().ContainKey("B-003");
    }

    [Then("the number of rows it has")]
    public void ThenTheMatrixExpectationCountsB003() =>
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC031")
            .Should().ContainSingle(Json).Which["expected"]!["rowsByClaim"]!["B-003"]!.GetValue<int>().Should().Be(2, Json);

    [Then("every matrix expectation requires one row and counts every declared claim")]
    public void ThenEveryMatrixExpectationCountsAllClaims()
    {
        var violations = Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC031");
        violations.Should().NotBeEmpty(Json).And.AllSatisfy(violation =>
        {
            violation["expected"]!["requiredRows"]!.GetValue<int>().Should().Be(1, Json);
            JsonNode.DeepEquals(violation["expected"]!["rowsByClaim"], _matrixCounts).Should().BeTrue(Json);
        });
    }

    private JsonNode IdentityExpectation =>
        Violations(static violation => violation["ruleId"]!.GetValue<string>() == "SPEC011").Should().ContainSingle(Json).Which["expected"]!;

    private int _firstClaimLine;

    private JsonObject? _matrixCounts;
}
