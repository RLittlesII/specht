using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Versioning;

/// <summary>
/// The embedded schema 0.3.0 under a manifest declaring no epic grammar (<c>0001-F7</c> B-037, B-038; <c>0001-F5</c>
/// decision 0004): its frontmatter schemas, loaded through a <see cref="MockFileSystem"/> and evaluated by the frontmatter
/// rules, accept a specification with no <c>epic</c> and an item whose <c>parent</c> is a bare Feature id.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SchemaVersionsEpicOptInUnitTests
{
    [Fact]
    public void ASpecificationWithNoEpicUnderNoEpicGrammar_WhenTheFeatureSchemaOfZeroThreeZeroEvaluatesIt_ShouldReportNothing()
    {
        // Given
        EngineServices.Versions().Find(new SemanticVersion(0, 3, 0)).Should().NotBeNull("the tool ships schema version 0.3.0");
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "schemaVersion": "0.3.0" }"""),
        });
        var schemas = EngineServices.Reader(files).ReadSchemas("repo", null);
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(DocumentParser.Parse(
            "---\nid: \"F1\"\ntype: feature\nspec_status: draft\nstatus: ready\npriority: med\nvalue: 4\nrisk: 3\nrank: 50\n"
                + "scored_by: \"groomed\"\nscored_on: \"2026-10-07\"\ndomain: \"Testing\"\nauthor: \"spec-author\"\nmilestone: null\n"
                + "children: []\ndepends_on: []\nblocks: []\nspikes: []\ncreated: \"2026-10-07\"\nupdated: \"2026-10-07\"\n"
                + "github_issue: null\nsynced_at: null\n---\n\n# Specification: F1\n",
            "src/sample/.spec/README.md"));
        SpechtModel model = new SpechtModelFixture().WithSchemas(schemas).WithFeatures(feature);

        // When
        var violations = new FeatureFrontmatterRule(new FrontmatterSchemaCheck()).Evaluate(model).ToList();

        // Then
        violations.Should().BeEmpty();
    }

    [Fact]
    public void AnItemWhoseParentIsABareFeatureIdUnderNoEpicGrammar_WhenTheItemSchemaOfZeroThreeZeroEvaluatesIt_ShouldReportNothing()
    {
        // Given
        EngineServices.Versions().Find(new SemanticVersion(0, 3, 0)).Should().NotBeNull("the tool ships schema version 0.3.0");
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "schemaVersion": "0.3.0" }"""),
        });
        var schemas = EngineServices.Reader(files).ReadSchemas("repo", null);
        ChildItem item = new ChildItemFixture().WithFrontmatter(DocumentParser.Parse(
            "---\nid: \"0001-01\"\nparent: \"F2\"\ntype: task\nstatus: ready\npriority: med\ncreated: \"2026-10-07\"\n"
                + "updated: \"2026-10-07\"\ngithub_issue: null\n---\n\n# Item\n",
            "src/sample/.spec/0001-01-task.md").Frontmatter);
        SpechtModel model = new SpechtModelFixture().WithSchemas(schemas).WithItems(item);

        // When
        var violations = new ItemFrontmatterRule(new FrontmatterSchemaCheck()).Evaluate(model).ToList();

        // Then
        violations.Should().BeEmpty();
    }

    private static SpechtDocumentParser DocumentParser => new(new FrontmatterReader());
}
