using Specht.Model;

namespace Specht.Tests.Report;

/// <summary>
/// Synthetic documents with no frontmatter, one per kind, for 0001-F3 B-026: a model under the schemas of
/// <see cref="SyntheticSchemaModelFixture"/>, the path of the schema the manifest names for the document's kind, and that
/// schema's required keys in its order.
/// </summary>
public sealed class MissingFrontmatters : TheoryData<string, SpecModel, string, string[]>
{
    public MissingFrontmatters()
    {
        Add(
            "a Feature specification",
            new SyntheticSchemaModelFixture().WithKind("feature").WithNode(null),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            ["priority", "id"]);
        Add(
            "an item",
            new SyntheticSchemaModelFixture().WithKind("task").WithNode(null),
            SyntheticSchemaModelFixture.ItemSchemaPath,
            ["parent", "id"]);
        Add(
            "an epic, whose schema requires no key",
            new SyntheticSchemaModelFixture().WithKind("epic").WithNode(null),
            SyntheticSchemaModelFixture.GroupSchemaPath,
            []);
    }
}
