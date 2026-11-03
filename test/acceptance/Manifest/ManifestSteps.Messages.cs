using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Manifest;

/// <summary>
/// Steps for the messages that name the manifest's values (<c>0001-F5</c> B-009; decision 0019). A renamed section, key or
/// marker is written into the default manifest in memory by the steps that rename it, the missing-test marker under
/// <c>markers.missing</c>; each specification here holds the
/// default sections with the claims or matrix section replaced, so the one violation a step reads is the one the scenario
/// names, and the message is read from the report the check gave.
/// </summary>
public sealed partial class ManifestSteps
{
    [Given("the root holds a specification with no {string} section")]
    public void GivenTheRootHoldsASpecificationWithNoSection(string title)
    {
        SpechtTree.Sections.Should().NotContain(section => section.StartsWith($"## {title}\n", StringComparison.Ordinal));
        Write("F1", SpechtTree.Sections);
    }

    [Given("the root holds a specification with a claim that has no row in {string}")]
    public void GivenTheRootHoldsASpecificationWithAClaimThatHasNoRowIn(string title) => WriteUnrowedClaim(title);

    [Given("the root holds a specification with a claim that has no row in its traceability table")]
    public void GivenTheRootHoldsASpecificationWithAClaimThatHasNoRowInItsTraceabilityTable() => WriteUnrowedClaim(DefaultTitles["matrix"]);

    [Given("the root holds a specification whose scenario tag names no claim under {string}")]
    public void GivenTheRootHoldsASpecificationWhoseScenarioTagNamesNoClaimUnder(string title) =>
        Tree.WriteFeatureFile(
            Write(
                "F1",
                SpechtTree.SectionsWith(
                    DefaultTitles["claims"],
                    $"## {title}\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n| B-001 | It does the thing. | brd | Active |\n")),
            PhantomTag);

    [Given("the manifest and on-disk schemas name the item id key {string}")]
    public void GivenTheManifestAndOnDiskSchemasNameTheItemIdKey(string key)
    {
        KeysOf("task")["id"] = key;
        EditSchema(SpechtTree.ItemSchemaPath, schema => RenameProperty(schema, "id", key));
    }

    [Given("the root holds an item whose {string} does not match its file name")]
    public void GivenTheRootHoldsAnItemWhoseKeyDoesNotMatchItsFileName(string key)
    {
        var path = Write("F1", SpechtTree.Sections);
        Tree.WriteItem(path, "0001-01", "0001-F1", idOverride: MisplacedItem);
        RenameLine(Path.Combine(Path.GetDirectoryName(path)!, "0001-01-item.md"), $"id: \"{MisplacedItem}\"", $"{key}: \"{MisplacedItem}\"");
    }

    [Then("the missing-section message names {string}")]
    public void ThenTheMissingSectionMessageNames(string title) =>
        _message = MessageOf("SPEC010", title).Should().Contain(title).And.Subject;

    [Then("it names the manifest by its path under the root")]
    public void ThenItNamesTheManifestByItsPathUnderTheRoot() =>
        (_message ?? throw new InvalidOperationException("No message was read.")).Should()
            .Contain(SpechtTree.ManifestPath)
            .And.NotContain(Tree.Root);

    [Then("the missing-row message names {string}")]
    public void ThenTheMissingRowMessageNames(string text) => MessageOf("SPEC031", "B-002").Should().Contain(text);

    [Then("the unresolved-tag message names {string}")]
    public void ThenTheUnresolvedTagMessageNames(string text) => MessageOf("SPEC021", "B-404").Should().Contain(text);

    [Then("the item-placement message names {string}")]
    public void ThenTheItemPlacementMessageNames(string key) => MessageOf("SPEC043", MisplacedItem).Should().Contain(key);

    private string MessageOf(string rule, string identifier)
    {
        _rejection.Should().BeNull();

        return _report!.Violations
            .Should()
            .ContainSingle(violation => violation.RuleId == RuleIds.Under(Manifest, rule) && violation.Identifier == identifier)
            .Which.Message;
    }

    private void WriteUnrowedClaim(string matrix)
    {
        var sections = SpechtTree.SectionsWith(DefaultTitles["claims"], TwoClaims).ToList();
        sections[sections.FindIndex(static section => section.StartsWith($"## {DefaultTitles["matrix"]}\n", StringComparison.Ordinal))] =
            Matrix(matrix, "Scenario | Test", "It does the thing | A test");
        Write("F1", sections);
    }

    private const string MisplacedItem = "0001-02";

    private string? _message;
}
