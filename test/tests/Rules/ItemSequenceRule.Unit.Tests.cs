using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC044 over a model built in memory (<c>0001-F1</c> B-004; item 0161): every reused item id before any gap, and per
/// epic only the first gap among the ids that carry a sequence number, whatever the levels of SPEC040, SPEC041 and
/// SPEC043. Under a schema 0.3.0 manifest declaring no epic grammar, an item whose folder holds no Feature takes no
/// place in any sequence (<c>0001-F5</c> B-070; decision 0020 call 5); discovery lists an item only beside a
/// specification, so only a model built in memory holds one.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ItemSequenceRuleUnitTests
{
    [Fact]
    public void AnItemWhoseFolderHoldsNoFeatureWithoutTheEpicGrammar_WhenTheItemSequenceRuleEvaluatesTheModel_ShouldLeaveItOutOfTheSequence()
    {
        // Given
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(new JsonObject { ["schemaVersion"] = "0.3.0" })
            .WithFeatures(new FeatureSpecFixture().WithDocument(
                new SpechtDocumentFixture().WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "F1" }))))
            .WithItems(new ChildItemFixture()
                .WithRelativePath("src/loose/.spec/0001-02-task.md")
                .WithFileName("0001-02-task.md")
                .WithParentDirectory("src/loose/.spec")
                .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "0001-02" })));
        var rule = new ItemSequenceRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Should().BeEmpty();
    }

    [Fact]
    public void ItemsWithAGapBeforeAReusedId_WhenTheItemSequenceRuleEvaluatesTheModel_ShouldReportSpec044OnTheReuseBeforeTheGap()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithItems(
            Item("src/sample/.spec/0001-01-task.md", "0001-01"),
            Item("src/sample/.spec/0001-03-task.md", "0001-03"),
            Item("src/other/.spec/0001-03-task.md", "0001-03"));
        var rule = new ItemSequenceRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Message)).Should().Equal(
            ("SPEC044", "src/other/.spec/0001-03-task.md", "item id '0001-03' is already used by src/sample/.spec/0001-03-task.md - ids are never reused"),
            ("SPEC044", "src/sample/.spec/0001-03-task.md", "epic 0001's item sequence skips 02 - tasks are numbered per epic, contiguously from 01"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void SequencesWithGapsUnnumberedIdsAndNoIds_WhenTheItemSequenceRuleEvaluatesTheModel_ShouldReportTheFirstGapPerEpicWhateverTheSiblingLevels(
        string manifest)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(manifest)!.AsObject()).WithItems(
            Item("src/sample/.spec/0001-01-task.md", "0001-01"),
            Item("src/sample/.spec/0001-03-task.md", "0001-03"),
            Item("src/sample/.spec/0001-05-task.md", "0001-05"),
            Item("src/sample/.spec/0002-01-task.md", "0002-01"),
            Item("src/sample/.spec/0002-1a-task.md", "0002-1a"),
            Item("src/sample/.spec/0003-02-task.md", "0003-02"),
            Item("src/sample/.spec/first-note.md", null),
            Item("src/sample/.spec/second-note.md", null));
        var rule = new ItemSequenceRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(
            ("SPEC044", "src/sample/.spec/0001-03-task.md"),
            ("SPEC044", "src/sample/.spec/0003-02-task.md"));
    }

    /// <summary>The manifest with every sibling id at its default level, and with every sibling id off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC040": "off", "SPEC041": "off", "SPEC043": "off" } }""");
        }
    }

    private static ChildItem Item(string path, string? id) =>
        new ChildItemFixture()
            .WithRelativePath(path)
            .WithFrontmatter(new FrontmatterFixture().WithNode(id is null ? new JsonObject() : new JsonObject { ["id"] = id }));
}
