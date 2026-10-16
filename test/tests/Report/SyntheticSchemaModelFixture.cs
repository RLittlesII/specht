using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// Builds a <see cref="SpecModel"/> under a synthetic manifest that renames all three frontmatter schema files, with
/// synthetic schemas loaded from a <see cref="MockFileSystem"/>, holding one Feature specification whose frontmatter the
/// synthetic Feature schema accepts, until a test overrides what it asserts on: the kind of the one document, its whole
/// frontmatter, or one key of it.
/// </summary>
/// <remarks>
/// Written on <see cref="AutoFixtureBase{TFixture}"/> rather than declared with <c>[AutoFixture]</c>: the document's kind
/// decides which of the model's three lists holds it, a choice the generated setters cannot make.
/// <see cref="SpecSchemaPath"/>, <see cref="ItemSchemaPath"/> and <see cref="GroupSchemaPath"/> are the paths the manifest
/// gives each kind's schema, which a violation's <c>expected</c> names.
/// </remarks>
internal sealed class SyntheticSchemaModelFixture : AutoFixtureBase<SyntheticSchemaModelFixture>
{
    /// <summary>The Feature schema's path under the root, by the synthetic manifest's file name.</summary>
    public const string SpecSchemaPath = SpecTree.FrontmatterSchemaFolder + "/spec.json";

    /// <summary>The item schema's path under the root, by the synthetic manifest's file name.</summary>
    public const string ItemSchemaPath = SpecTree.FrontmatterSchemaFolder + "/item.json";

    /// <summary>The epic schema's path under the root, by the synthetic manifest's file name.</summary>
    public const string GroupSchemaPath = SpecTree.FrontmatterSchemaFolder + "/group.json";

    /// <summary>Sets the kind of the one document: <c>feature</c>, <c>task</c> or <c>epic</c>.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The fixture.</returns>
    public SyntheticSchemaModelFixture WithKind(string kind) => With(ref _kind, kind);

    /// <summary>Sets the document's whole frontmatter mapping.</summary>
    /// <param name="node">The mapping, or <see langword="null"/> for a document with none.</param>
    /// <returns>The fixture.</returns>
    public SyntheticSchemaModelFixture WithNode(JsonObject? node) => With(ref _node, node);

    /// <summary>Sets one key of the document's frontmatter, keeping the others.</summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>The fixture.</returns>
    public SyntheticSchemaModelFixture WithKey(string key, JsonNode? value)
    {
        var node = _node?.DeepClone().AsObject() ?? new JsonObject();
        node[key] = value;
        return WithNode(node);
    }

    /// <summary>Removes one key from the document's frontmatter, keeping the others.</summary>
    /// <param name="key">The key.</param>
    /// <returns>The fixture.</returns>
    public SyntheticSchemaModelFixture WithoutKey(string key)
    {
        var node = _node?.DeepClone().AsObject() ?? new JsonObject();
        node.Remove(key);
        return WithNode(node);
    }

    public static implicit operator SpecModel(SyntheticSchemaModelFixture fixture) => fixture.Build();

    private SpecModel Build()
    {
        Frontmatter frontmatter = new FrontmatterFixture().WithNode(_node?.DeepClone().AsObject());
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(
                """
                {
                  "identifiers": { "epic": "^[0-9]{4}$" },
                  "frontmatterSchemas": { "feature": "spec.json", "task": "item.json", "epic": "group.json" }
                }
                """),
            [Path.Combine("repo", SpecSchemaPath)] = new(SpecSchema),
            [Path.Combine("repo", ItemSchemaPath)] = new(ItemSchema),
            [Path.Combine("repo", GroupSchemaPath)] = new(GroupSchema),
        });
        var model = new SpecModelFixture().WithSchemas(EngineServices.LoadOnDiskSchemas(files, "repo"));

        return _kind switch
        {
            "feature" => model.WithFeatures(new FeatureSpecFixture().WithDocument(new SpecDocumentFixture().WithFrontmatter(frontmatter))),
            "task" => model.WithItems(new ChildItemFixture().WithFrontmatter(frontmatter)),
            "epic" => model.WithList([(EpicFile)new EpicFileFixture().WithFrontmatter(frontmatter)]),
            _ => throw new ArgumentOutOfRangeException(nameof(_kind), _kind, "The kind is feature, task or epic."),
        };
    }

    private const string SpecSchema =
        """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "required": ["priority", "id"],
          "properties": {
            "id": { "type": "string" },
            "priority": { "enum": ["low", "high"] },
            "value": { "type": "integer", "minimum": 0 },
            "name": { "type": "string", "minLength": 3, "pattern": "^[a-z]+$" },
            "scored_by": {},
            "synced_at": { "type": "string", "format": "date-time" },
            "kind": {},
            "mode": {},
            "choice": { "oneOf": [{ "const": "x" }, { "type": "integer" }] },
            "budget": { "$ref": "#/$defs/amount" },
            "meta": { "$ref": "https://json-schema.org/draft/2020-12/meta/validation" }
          },
          "$defs": { "amount": { "type": "integer", "minimum": 0 } },
          "additionalProperties": false,
          "allOf": [
            {
              "if": { "properties": { "scored_by": { "type": "string" } }, "required": ["scored_by"] },
              "then": { "properties": { "value": { "minimum": 1 } }, "required": ["synced_at"] }
            },
            { "not": { "properties": { "kind": { "const": "legacy" } }, "required": ["kind"] } },
            { "anyOf": [{ "properties": { "mode": { "const": "a" } } }, { "properties": { "mode": { "const": "b" } } }] }
          ]
        }
        """;

    private const string ItemSchema =
        """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "required": ["parent", "id"],
          "properties": { "priority": { "enum": ["med"] } }
        }
        """;

    private const string GroupSchema =
        """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "properties": { "priority": { "const": "med" } }
        }
        """;

    private string _kind = "feature";

    private JsonObject? _node = new()
    {
        ["id"] = "F1",
        ["priority"] = "low",
        ["value"] = 1,
        ["scored_by"] = "someone",
        ["synced_at"] = "2026-10-07T00:00:00Z",
    };
}
