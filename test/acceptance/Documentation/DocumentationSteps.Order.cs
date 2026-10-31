using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Documentation;

public sealed partial class DocumentationSteps
{
    [Given("a documentation comment with {string} binding elements of one role")]
    public void GivenOrderedComment(string existing)
    {
        _repairRecords = OrderedRecords;
        _orderExisting = existing switch
        {
            "epic 0201 claim" => ["<verifies epic=\"0201\" feature=\"F2\" claim=\"B-001\" />"],
            "Feature F10 claim" => ["<verifies epic=\"0001\" feature=\"F10\" claim=\"B-001\" />"],
            "epic 0201 ADR" => ["<follows epic=\"0201\" feature=\"F2\" adr=\"ADR-0001\" />"],
            _ => ["<verifies claim=\"B-010\" />", "<verifies claim=\"B-002\" />"],
        };
    }

    [When("{string} writes a binding element into that comment")]
    public async Task WriteOrderedBinding(string repair)
    {
        var lines = new List<string>(_orderExisting);
        if (repair == "Fix All unambiguous default materialization")
        {
            _repairBulk = true;
            _repairKey = "Specht.MaterializeDefault";
            _repairSource = "/// <spec epic=\"0001\" feature=\"F2\" />\n" + Comment(lines);
            _repairExpected = "/// <spec epic=\"0001\" feature=\"F2\" />\n" + Comment([
                "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-002\" />", "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-010\" />"]);
        }
        else
        {
            var role = repair.Contains("ADR", StringComparison.Ordinal) ? "follows" : "verifies";
            var written = role == "follows" ? "<follows epic=\"0001\" feature=\"F2\" adr=\"ADR-0001\" />"
                : "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />";
            lines.Add(role == "follows" ? "<follows epic=\"0001\" feature=\"F2\" adr=\"ADR-0099\" />"
                : "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-999\" />");
            _repairKey = "Specht.Select:" + role + ":" + written;
            _repairSource = Comment(lines);
            _repairExpected = Comment([written, .. _orderExisting]);
        }

        await AnalyzeRepair();
    }

    [Then("that comment's binding elements of that role are ordered by epic, then feature, then claim or ADR")]
    public void ThenOrdered() => _repairFixed.Should().Be(_repairExpected);

    [Then("{string} precedes {string}")]
    public void ThenPrecedes(string earlier, string later)
    {
        var first = _repairFixed!.IndexOf(OrderedElements[earlier], StringComparison.Ordinal);
        first.Should().BeGreaterThanOrEqualTo(0);
        first.Should().BeLessThan(_repairFixed.IndexOf(OrderedElements[later], StringComparison.Ordinal));
    }

    [Given("a documentation comment with summary text and a binding element sharing its line with other content")]
    public void GivenOrderBoundary()
    {
        _repairRecords = OrderedRecords;
        _repairSource = "public class Example {\n    /// <summary>Keep</summary>\n"
            + "    /// <verifies epic=\"0201\" feature=\"F2\" claim=\"B-001\" />\n"
            + "    /// <remarks>Keep</remarks><verifies epic=\"0001\" feature=\"F10\" claim=\"B-001\" />\n"
            + "    /// <verifies epic=\"0001\" feature=\"F2\" claim=\"B-999\" />\n    public void Run() {}\n}";
    }

    [When("a repair writes a binding element into that comment")]
    public Task WriteBoundaryBinding()
    {
        _repairKey = "Specht.Select:verifies:<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />";
        return AnalyzeRepair();
    }

    [Then("the summary text and the shared-line binding element keep their places")]
    public void ThenOutsideOrderKept()
    {
        var before = _repairSource!.Split('\n');
        var after = _repairFixed!.Split('\n');
        _repairFixed.Should().Contain("<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />");
        after[1].Should().Be(before[1]);
        after[3].Should().Be(before[3]);
    }

    [Given("existing records for {string} and {string}")]
    public void GivenOrderedRecords(string first, string second)
    {
        _repairRecords = OrderedRecords;
        OrderedChoices.Should().ContainKeys(first, second);
    }

    [Given("a binding repair that offers target choices")]
    public void GivenChoiceRepair()
    {
        _repairSource = "public class Example {\n    public void Run() {}\n}";
        _repairKey = null;
    }

    [When("the target choices are listed")]
    public Task ListChoices() => AnalyzeRepair();

    [Then("the choice for {string} is listed before the choice for {string}")]
    public void ThenChoiceOrder(string first, string second)
    {
        var titles = _repairActions.Select(static action => action.Title).ToList();
        var earlier = titles.IndexOf(OrderedChoices[first]);
        earlier.Should().BeGreaterThanOrEqualTo(0);
        earlier.Should().BeLessThan(titles.IndexOf(OrderedChoices[second]));
    }

    private static string Comment(IEnumerable<string> elements) =>
        "public class Example {\n" + string.Concat(elements.Select(static element => "    /// " + element + "\n")) + "    public void Run() {}\n}";

    private static readonly Dictionary<string, string> OrderedElements = new(StringComparer.Ordinal)
    {
        ["the epic 0001 claim"] = "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />",
        ["the epic 0201 claim"] = "<verifies epic=\"0201\" feature=\"F2\" claim=\"B-001\" />",
        ["the Feature F2 claim"] = "<verifies epic=\"0001\" feature=\"F2\" claim=\"B-001\" />",
        ["the Feature F10 claim"] = "<verifies epic=\"0001\" feature=\"F10\" claim=\"B-001\" />",
        ["the epic 0001 ADR"] = "<follows epic=\"0001\" feature=\"F2\" adr=\"ADR-0001\" />",
        ["the epic 0201 ADR"] = "<follows epic=\"0201\" feature=\"F2\" adr=\"ADR-0001\" />",
        ["the lower claim id"] = "claim=\"B-002\"",
        ["the higher claim id"] = "claim=\"B-010\"",
    };

    private static readonly Dictionary<string, string> OrderedChoices = new(StringComparer.Ordinal)
    {
        ["epic 0001"] = "Bind verifies to 0001 / F2 / B-001",
        ["epic 0201"] = "Bind verifies to 0201 / F2 / B-001",
        ["Feature F2"] = "Bind verifies to 0001 / F2 / B-001",
        ["Feature F10"] = "Bind verifies to 0001 / F10 / B-001",
        ["claim B-002"] = "Bind verifies to 0001 / F2 / B-002",
        ["claim B-010"] = "Bind verifies to 0001 / F2 / B-010",
    };

    private static readonly (string Kind, string Path, string Text)[] OrderedRecords =
    [
        ("configuration", "config/consumer.json", """
         {"schemaVersion":"0.1.0","identifiers":{"epic":"^[0-9]{4}$","feature":"^F[0-9]+$","claim":"^B-[0-9]{3}$","adr":"^ADR-[0-9]{4}$"},
          "claimsSection":"3. Acceptance Criteria","markers":{},"codeAnalysis":{
           "mappings":{"/schemaVersion":"/schemaVersion","/identifiers":"/identifiers","/claimsSection":"/claimsSection",
             "/policy":"/codeAnalysis","/markers":"/markers"},
           "projects":[{"path":"test/tests.csproj","targetFrameworks":["net10.0"]}],"features":[{"epic":"0001","feature":"F2"}],
           "enforcement":[{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["testMethod"]}],
           "records":{"adrProfiles":[{"glob":"src/*/.spec/adr/*.md","owner":"containingFeature",
             "identity":{"headingLevel":1,"separator":": "},"lifecycle":{"section":"Status","firstWord":true}}]}}}
         """),
        ("feature", "src/a/.spec/README.md", "---\nid: F2\nepic: \"0201\"\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | Synthetic | agreement | Active |\n"),
        ("feature", "src/b/.spec/README.md", "---\nid: F10\nepic: \"0001\"\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-001 | Synthetic | agreement | Active |\n"),
        ("feature", "src/c/.spec/README.md", "---\nid: F2\nepic: \"0001\"\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
            + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| B-010 | Later | agreement | Active |\n"
            + "| B-002 | Earlier | agreement | Active |\n| B-001 | First | agreement | Active |\n"),
        ("adr", "src/a/.spec/adr/0001.md", "# ADR-0001: Synthetic decision of epic 0201\n## Status\nAccepted, on the agreed date.\n"),
        ("adr", "src/c/.spec/adr/0001.md", "# ADR-0001: Synthetic decision of epic 0001\n## Status\nAccepted, on the agreed date.\n"),
        ("schema:feature", "contracts/feature.json", "{\"type\":\"object\"}"),
    ];

    private string[] _orderExisting = [];
}
