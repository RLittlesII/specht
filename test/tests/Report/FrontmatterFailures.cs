using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Tests.Report;

/// <summary>
/// Synthetic frontmatters the schemas of <see cref="SyntheticSchemaModelFixture"/> reject, for 0001-F3 B-009: a model, the
/// path of the schema its document's kind reads, the rule and the key of the violations to read (no key: the frontmatter
/// as a whole), and the <c>constraints</c> each of those violations carries, one per violation in any order: the assertions
/// that failed at its place in the schema with their values there, or none.
/// </summary>
public sealed class FrontmatterFailures : TheoryData<string, SpecModel, string, string, string?, JsonObject[]>
{
    public FrontmatterFailures()
    {
        Add(
            "a value outside an enum",
            new SyntheticSchemaModelFixture().WithKey("priority", "urgent"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "priority",
            [new() { ["enum"] = new JsonArray("low", "high") }]);
        Add(
            "a value of another type",
            new SyntheticSchemaModelFixture().WithKey("value", "high"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "value",
            [new() { ["type"] = "integer" }]);
        Add(
            "two assertions failing at one place",
            new SyntheticSchemaModelFixture().WithKey("name", string.Empty),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "name",
            [new() { ["minLength"] = 3, ["pattern"] = "^[a-z]+$" }]);
        Add(
            "a value that does not follow its format",
            new SyntheticSchemaModelFixture().WithKey("synced_at", "yesterday"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "synced_at",
            [new() { ["format"] = "date-time" }]);
        Add(
            "a required key absent at the root, beside an assertion under a not",
            new SyntheticSchemaModelFixture().WithoutKey("priority"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            null,
            [new() { ["required"] = new JsonArray("priority", "id") }, []]);
        Add(
            "an applicator failing at the root, beside an assertion under a not",
            new SyntheticSchemaModelFixture().WithKey("priority", "urgent"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            null,
            [[], []]);
        Add(
            "an assertion under a then the document must satisfy",
            new SyntheticSchemaModelFixture().WithKey("value", 0),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "value",
            [new() { ["minimum"] = 1 }]);
        Add(
            "a required key under a then, beside the allOf and then applicators and an assertion under a not",
            new SyntheticSchemaModelFixture().WithoutKey("synced_at"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            null,
            [[], [], new() { ["required"] = new JsonArray("synced_at") }, []]);
        Add(
            "an assertion under an if the document need not satisfy",
            new SyntheticSchemaModelFixture().WithKey("priority", "urgent").WithKey("scored_by", 5),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "scored_by",
            [[]]);
        Add(
            "a not whose subschema passed, beside the allOf applicator",
            new SyntheticSchemaModelFixture().WithKey("kind", "legacy"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            null,
            [[], []]);
        Add(
            "an assertion in each failed anyOf branch",
            new SyntheticSchemaModelFixture().WithKey("mode", "c"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "mode",
            [[], []]);
        Add(
            "an assertion in each failed oneOf branch",
            new SyntheticSchemaModelFixture().WithKey("choice", "y"),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "choice",
            [[], [], []]);
        Add(
            "an assertion at the place a same-document reference resolves to",
            new SyntheticSchemaModelFixture().WithKey("budget", -1),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "budget",
            [new() { ["minimum"] = 0 }]);
        Add(
            "an assertion in a document a reference names",
            new SyntheticSchemaModelFixture().WithKey("meta", 5),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "meta",
            [[]]);
        Add(
            "a key the schema does not declare",
            new SyntheticSchemaModelFixture().WithKey("extra", 1),
            SyntheticSchemaModelFixture.SpecSchemaPath,
            "SPEC002",
            "extra",
            [[]]);
        Add(
            "an item's value outside its schema's enum",
            new SyntheticSchemaModelFixture().WithKind("task").WithNode(new JsonObject { ["parent"] = "F1", ["id"] = "0001-01", ["priority"] = "urgent" }),
            SyntheticSchemaModelFixture.ItemSchemaPath,
            "SPEC003",
            "priority",
            [new() { ["enum"] = new JsonArray("med") }]);
        Add(
            "an epic's value other than its schema's const",
            new SyntheticSchemaModelFixture().WithKind("epic").WithNode(new JsonObject { ["priority"] = "urgent" }),
            SyntheticSchemaModelFixture.GroupSchemaPath,
            "SPEC004",
            "priority",
            [new() { ["const"] = "med" }]);
    }
}
