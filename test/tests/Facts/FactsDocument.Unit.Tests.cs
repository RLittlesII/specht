using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Facts;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Facts;

/// <summary>
/// The facts document projected from a model built in memory (<c>0132-F2</c> B-001, B-003, B-004, B-017, B-022, B-026):
/// every Feature specification in the model's order and no epic, each with the path, identity, statuses and dependency
/// lists its frontmatter gives, the same members under two pins, and the same text for the same model.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FactsDocumentUnitTests
{
    [Fact]
    public void FeaturesInTheModelsOrder_WhenTheDocumentIsMade_ShouldListEverySpecificationInThatOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/zeta/.spec/README.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("epics/0001-epic/F1-feature/spec.md")),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/alpha/.spec/README.md")));

        // When
        var document = JsonNode.Parse(FactsDocument.From(model).ToJson())!;

        // Then
        document["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>()).Should().Equal(
            "src/zeta/.spec/README.md", "epics/0001-epic/F1-feature/spec.md", "src/alpha/.spec/README.md");
    }

    [Fact]
    public void AnEpicFileBesideAFeature_WhenTheDocumentIsMade_ShouldGiveOnlyTheFeature()
    {
        // Given
        EpicFile epic = new EpicFileFixture().WithRelativePath("epics/0001-epic/epic.md");
        SpechtModel model = new SpechtModelFixture()
            .WithList([epic])
            .WithFeatures(new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("epics/0001-epic/F1-feature/spec.md")));

        // When
        var document = JsonNode.Parse(FactsDocument.From(model).ToJson())!;

        // Then
        document["specifications"]!.AsArray().Select(static specification => specification!["path"]!.GetValue<string>()).Should().Equal(
            "epics/0001-epic/F1-feature/spec.md");
    }

    [Fact]
    public void AFeatureWithAnIdentity_WhenTheDocumentIsMade_ShouldCarryItsPathQualifiedIdAndBothStatuses()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/facts/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/facts/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0132",
                        ["id"] = "F2",
                        ["spec_status"] = "approved",
                        ["status"] = "in-progress",
                    }))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["path"]!.GetValue<string>().Should().Be("src/facts/.spec/README.md");
        specification["id"]!.GetValue<string>().Should().Be("0132-F2");
        specification["specStatus"]!.GetValue<string>().Should().Be("approved");
        specification["status"]!.GetValue<string>().Should().Be("in-progress");
    }

    [Fact]
    public void DependencyListsTheCheckCannotResolve_WhenTheDocumentIsMade_ShouldCarryEachAsTheFrontmatterWritesIt()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["depends_on"] = new JsonArray("F9", "0002/F3"),
                        ["blocks"] = new JsonArray("F4", "F2"),
                        ["children"] = new JsonArray("0400"),
                    }))));

        // When
        var specification = JsonNode.Parse(FactsDocument.From(model).ToJson())!["specifications"]!.AsArray().Should().ContainSingle().Which!;

        // Then
        specification["dependsOn"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F9", "0002/F3");
        specification["blocks"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("F4", "F2");
        specification["children"]!.AsArray().Select(static value => value!.GetValue<string>()).Should().Equal("0400");
    }

    [Theory]
    [ClassData(typeof(FactsPinPairs))]
    public void TheSameFeaturesUnderTwoPins_WhenEachDocumentIsMade_ShouldCarryTheSameMembers(string firstPin, string secondPin)
    {
        // Given
        FeatureSpec[] features =
        [
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["spec_status"] = "draft",
                        ["status"] = "ready",
                        ["depends_on"] = new JsonArray("F2"),
                    }))),
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/second/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/second/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "F2" }))),
        ];
        SpechtModel first = new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = firstPin }).WithFeatures(features);
        SpechtModel second = new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = secondPin }).WithFeatures(features);

        // When
        var documents = new[] { first, second }.Select(static model => JsonNode.Parse(FactsDocument.From(model).ToJson())!).ToArray();

        // Then
        var members = documents.Select(static document => document["specifications"]!.AsArray()
            .Select(static specification => string.Join(',', specification!.AsObject().Select(static member => member.Key)))
            .Prepend(string.Join(',', document.AsObject().Select(static member => member.Key)))
            .ToArray()).ToArray();
        members[0].Should().HaveCount(3);
        members[1].Should().Equal(members[0]);
    }

    [Fact]
    public void TheSameFeaturesReadIntoTwoModels_WhenEachDocumentIsSerialized_ShouldGiveTheSameText()
    {
        // Given
        var models = Enumerable.Range(0, 2).Select(static _ => (SpechtModel) new SpechtModelFixture().WithFeatures(
            new FeatureSpecFixture()
                .WithLocation(new SpechtLocationFixture().WithRelativePath("src/first/.spec/README.md"))
                .WithDocument(new SpechtDocumentFixture()
                    .WithRelativePath("src/first/.spec/README.md")
                    .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
                    {
                        ["epic"] = "0001",
                        ["id"] = "F1",
                        ["spec_status"] = "draft",
                        ["status"] = "ready",
                        ["depends_on"] = new JsonArray("F2"),
                        ["blocks"] = new JsonArray(),
                    }))),
            new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/second/.spec/README.md")))).ToArray();

        // When
        var first = FactsDocument.From(models[0]).ToJson();
        var again = FactsDocument.From(models[0]).ToJson();
        var second = FactsDocument.From(models[1]).ToJson();

        // Then
        first.Should().NotBeEmpty();
        again.Should().Be(first);
        second.Should().Be(first);
    }
}
