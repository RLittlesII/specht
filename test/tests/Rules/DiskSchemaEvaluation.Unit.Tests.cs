using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>Unresolved local references fail as controlled disk-schema errors when used (0001-F7 B-105 to B-107).</summary>
[Trait("Tier", "Unit")]
public sealed class DiskSchemaEvaluationUnitTests
{
    [Theory]
    [InlineData("feature")]
    [InlineData("task")]
    [InlineData("epic")]
    public void ASelectedDiskSchemaWithAnUnresolvedReference_WhenUsed_ShouldRejectItNamingItsRelativeFile(string kind)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "identifiers": { "epic": "^[0-9]{4}$" } }"""),
            ["repo/" + SpechtTree.FeatureSchemaPath] = new("{}"),
            ["repo/" + SpechtTree.ItemSchemaPath] = new("{}"),
            ["repo/" + SpechtTree.EpicSchemaPath] = new("{}"),
        });
        var relative = SpechtTree.FrontmatterSchemaFolder + "/" + kind + ".schema.json";
        files.AddFile("repo/" + relative, new MockFileData("{ \"$ref\": \"#/missing\" }"));
        var schemas = EngineServices.Schemas(files).Load("repo", SchemaSourceSelection.Disk);
        Frontmatter frontmatter = new FrontmatterFixture().WithNode(new JsonObject());
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(new SpechtDocumentFixture().WithFrontmatter(frontmatter));
        ChildItem task = new ChildItemFixture().WithFrontmatter(frontmatter);
        EpicFile epic = new EpicFileFixture().WithFrontmatter(frontmatter);
        SpechtModel model = new SpechtModelFixture().WithSchemas(schemas).WithFeatures(feature).WithItems(task).WithList([epic]);
        var check = new FrontmatterSchemaCheck();
        ISpechtRule[] rules = [new FeatureFrontmatterRule(check), new ItemFrontmatterRule(check), new EpicFrontmatterRule(check)];

        // When
        var evaluate = () => rules.SelectMany(rule => rule.Evaluate(model)).ToList();

        // Then
        evaluate.Should().Throw<SpechtManifestException>().Which.Message
            .Should().Contain(relative).And.NotContain(files.Path.GetFullPath("repo"));
    }
}
