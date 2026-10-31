using System.IO;
using System.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.Versioning;

public sealed partial class VersioningSteps
{
    [Given("the root holds a specification whose frontmatter carries no epic")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterCarriesNoEpic()
    {
        var path = Tree.WriteCoLocatedFeature(NoEpicArea, "0001", "F1");
        File.WriteAllLines(path, File.ReadAllLines(path).Where(static line => !line.StartsWith("epic:", StringComparison.Ordinal)));
        File.ReadAllText(path).Should().NotContain("epic:");
    }

    [Given("the root holds an item whose parent is {string}")]
    public void GivenTheRootHoldsAnItemWhoseParentIs(string parent)
    {
        var specification = Tree.WriteCoLocatedFeature(NoEpicArea, "0001", parent);
        Tree.WriteItem(specification, NoEpicItem, parent);
    }

    [Then("no frontmatter violation is reported for the specification")]
    public void ThenNoFrontmatterViolationIsReportedForTheSpecification()
    {
        Document["layouts"]!.AsArray().Sum(static layout => layout!["specificationCount"]!.GetValue<int>()).Should().Be(1, _stderr);
        Violations.Should().NotContain(static violation =>
            violation["file"]!.GetValue<string>() == NoEpicSpecificationPath && FrontmatterRules.Contains(violation["ruleId"]!.GetValue<string>()));
    }

    [Then("no frontmatter violation is reported for the item")]
    public void ThenNoFrontmatterViolationIsReportedForTheItem()
    {
        Document["itemCount"]!.GetValue<int>().Should().Be(1, _stderr);
        Violations.Should().NotContain(static violation =>
            violation["file"]!.GetValue<string>() == NoEpicItemPath && FrontmatterRules.Contains(violation["ruleId"]!.GetValue<string>()));
    }

    private const string NoEpicArea = "src/area";

    private const string NoEpicItem = "0001-01";

    private const string NoEpicSpecificationPath = NoEpicArea + "/.spec/README.md";

    private const string NoEpicItemPath = NoEpicArea + "/.spec/" + NoEpicItem + "-item.md";
}
