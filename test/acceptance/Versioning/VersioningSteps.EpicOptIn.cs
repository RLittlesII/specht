using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// Epics left out under schema 0.3.0 (0001-F7 B-037, B-038; 0001-F5 decision 0004), item 0321. The specification is
/// co-located at <c>src/sample/.spec/README.md</c>; "carries no epic" removes the <c>epic</c> line the tree writes, and the
/// item sits beside the specification of the Feature its parent names. "The check runs" launches the built tool with
/// <c>--json</c>, and each Then requires the check to have completed before reading the frontmatter rules' violations for
/// that one file.
/// </summary>
public sealed partial class VersioningSteps
{
    [Given("the root holds a specification whose frontmatter carries no epic")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterCarriesNoEpic()
    {
        var path = Tree.WriteCoLocatedFeature("src/sample", "0001", "F1");
        var lines = File.ReadAllLines(path).ToList();
        lines.RemoveAll(static line => line.StartsWith("epic:", StringComparison.Ordinal)).Should().Be(1);
        File.WriteAllText(path, string.Join('\n', lines) + "\n");
        _optInPath = SamplePath;
    }

    [Given("the root holds an item whose parent is {string}")]
    public void GivenTheRootHoldsAnItemWhoseParentIs(string parent)
    {
        var specPath = Tree.WriteCoLocatedFeature("src/sample", "0001", parent);
        Tree.WriteItem(specPath, "0001-01", parent);
        _optInPath = "src/sample/.spec/0001-01-item.md";
    }

    [Then("no frontmatter violation is reported for the specification")]
    [Then("no frontmatter violation is reported for the item")]
    public void ThenNoFrontmatterViolationIsReportedForTheOptInFile()
    {
        var path = _optInPath ?? throw new InvalidOperationException("The root holds no specification or item to read.");
        ThenTheCheckCompletes();
        Violations.Should().NotContain(violation =>
            violation["file"]!.GetValue<string>() == path && FrontmatterRules.Contains(violation["ruleId"]!.GetValue<string>()));
    }

    private string? _optInPath;
}
