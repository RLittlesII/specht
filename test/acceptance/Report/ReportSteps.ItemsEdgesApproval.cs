using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Report;

public sealed partial class ReportSteps
{
    [Given("the root holds a specification whose children skip from 0001-02 to 0001-04")]
    public void GivenTheItemsSkipTheThirdNumber()
    {
        var path = Tree.WriteFeature("0001", "F1");
        foreach (var id in new[] { "0001-01", "0001-02", "0001-04" })
        {
            Tree.WriteItem(path, id, "0001-F1");
        }
    }

    [Given("the root declares a missing {word} item")]
    public void GivenADeclaredItemIsMissing(string key) =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { [key] = "[\"0001-01\"]" });

    [Given("the root holds Feature F1 depending on F2")]
    public void GivenF1DependsOnF2() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });

    [Given("F2 declares no blocks edge back to F1")]
    public void GivenF2HasNoReciprocalEdge() => Tree.WriteFeature("0001", "F2");

    [Given("the root holds Features F1 and F2 in one epic")]
    public void GivenTheRootHoldsTwoFeatures()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.WriteFeature("0001", "F2");
    }

    [Given("F1 depends on F9")]
    public void GivenF1DependsOnNothing() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["depends_on"] = "[\"F9\"]" });

    [Given("the root holds F1 depending on F2 and F2 depending on itself")]
    public void GivenTheDependencyPathLeadsIntoASelfCycle()
    {
        GivenF1DependsOnF2();
        Tree.WriteFeature("0001", "F2", new Dictionary<string, string> { ["depends_on"] = "[\"F2\"]" });
    }

    [Given("the root holds two items both declaring the id 0001-02")]
    public void GivenTwoItemsReuseAnId()
    {
        var path = Tree.WriteFeature("0001", "F1");
        Tree.WriteItem(path, "0001-02", "0001-F1", slug: "first");
        Tree.WriteItem(path, "0001-02", "0001-F1", slug: "second");
    }

    [Given("the root holds an approved specification with one missing cell in its traceability table")]
    public void GivenApprovedCoverageIsMissing() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "approved" },
            sections: SpecTree.SectionsWith("9. Traceability Matrix",
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| - | - | - | - |\n"
                + "| B-001 | It does the thing | Missing | Covered |\n"));

    [Given("the root holds an approved specification with a blocked sign-off row")]
    public void GivenApprovedSignOffIsBlocked() =>
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["spec_status"] = "approved" },
            sections: SpecTree.SectionsWith("12. Sign-off",
                "## 12. Sign-off\n\n| Section | Status | Reviewer | Note |\n| - | - | - | - |\n"
                + "| 1-5 | 🟢 | spec-reviewer | Ready |\n| 6-9 | 🔴 | spec-reviewer | Blocked |\n"));

    [Then("the violation's expectation names 0001-03 as the next number in the sequence")]
    public void ThenTheGapNamesItsNextNumber() =>
        AssertRepairExpectation("SPEC044", new JsonObject { ["fileName"] = "0001-03-*.md", ["nextNumber"] = 3 });

    [Then("the {word} expectation names the declared item's file shape")]
    public void ThenTheDeclaredItemNamesItsFileShape(string ruleId)
    {
        var expected = new JsonObject { ["fileName"] = "0001-01-*.md" };
        if (ruleId == "SPEC041")
        {
            expected["type"] = "spike";
        }

        AssertRepairExpectation(ruleId, expected);
    }

    [Then("the violation's expectation carries the edge F2 must declare")]
    public void ThenTheMissingEdgeNamesItsDeclaration() =>
        AssertRepairExpectation("SPEC051", new JsonObject
        {
            ["edge"] = new JsonObject { ["feature"] = "0001-F2", ["key"] = "blocks", ["value"] = "0001-F1" },
        });

    [Then("the violation's expectation carries F1 and F2 as the Features a dependency could name")]
    public void ThenTheUnknownDependencyNamesItsTargets() =>
        AssertRepairExpectation("SPEC050", new JsonObject { ["features"] = new JsonArray("0001-F1", "0001-F2") });

    [Then("the F1 cycle expectation lists F1, F2 and F2 in order")]
    public void ThenTheCyclePreservesItsLeadIn() =>
        AssertRepairExpectation("SPEC052", new JsonObject { ["cycle"] = new JsonArray("0001-F1", "0001-F2", "0001-F2") },
            "dependency cycle: 0001-F1 ->");

    [Then("the violation's expectation names the file of the item that declared 0001-02 first")]
    public void ThenTheReusedIdNamesItsFirstHolder() =>
        AssertRepairExpectation("SPEC044", new JsonObject
        {
            ["fileName"] = "0001-02-*.md",
            ["firstDeclaration"] = "epics/0001-epic/F1-feature/0001-02-first.md",
        }, "already used");

    [Then("the violation's expectation names the cell that must change before the approved status is honest")]
    public void ThenTheMissingCellNamesItsPosition() =>
        AssertRepairExpectation("SPEC060", new JsonObject { ["section"] = "9. Traceability Matrix", ["row"] = 1, ["columns"] = new JsonArray(3) });

    [Then("the approval expectation names the sign-off row that must change")]
    public void ThenTheSignOffNamesItsRow() =>
        AssertRepairExpectation("SPEC061", new JsonObject { ["section"] = "12. Sign-off", ["row"] = 2 });

    private void AssertRepairExpectation(string ruleId, JsonObject expected, string? messagePart = null)
    {
        var actual = Violations(violation => violation["ruleId"]!.GetValue<string>() == ruleId
            && (messagePart is null || violation["message"]!.GetValue<string>().Contains(messagePart, StringComparison.Ordinal)))
            .Should().ContainSingle(Json).Which["expected"];
        JsonNode.DeepEquals(actual, expected).Should().BeTrue(Json);
    }
}
