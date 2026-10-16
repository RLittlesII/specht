using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// What a violation's <c>expected</c> carries for the frontmatter, section and table rules (<c>0001-F3</c> B-009, B-010,
/// B-011, B-026; decision 0004), over a model built in memory under a synthetic manifest and synthetic on-disk schemas,
/// evaluated by <see cref="SpechtRunner.Evaluate"/> and made a document by <see cref="SpecReportDocument.From"/>. The
/// manifest names its own schema files, sections, roles and headers, so a value the document carries is the manifest's and
/// not the default's. The schema sources are the integration tier's.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecReportDocumentExpectedUnitTests
{
    /// <summary>
    /// Gets a document kind, its frontmatter, the rule and the key of the violations to read (no key: the frontmatter as a
    /// whole), and the <c>constraints</c> each of those violations carries, one per violation in any order: the assertions
    /// that failed at its place in the schema with their values there, or none.
    /// </summary>
    public static TheoryData<string, string, JsonObject, string, string?, JsonObject[]> FrontmatterFailures =>
        new()
        {
            { "a value outside an enum", "feature", Valid(("priority", "urgent")), "SPEC002", "priority", [Enum("low", "high")] },
            { "a value of another type", "feature", Valid(("value", "high")), "SPEC002", "value", [new() { ["type"] = "integer" }] },
            {
                "two assertions failing at one place",
                "feature",
                Valid(("name", string.Empty)),
                "SPEC002",
                "name",
                [new() { ["minLength"] = 3, ["pattern"] = "^[a-z]+$" }]
            },
            {
                "a value that does not follow its format",
                "feature",
                Valid(("synced_at", "yesterday")),
                "SPEC002",
                "synced_at",
                [new() { ["format"] = "date-time" }]
            },
            {
                "a required key absent at the root, beside an assertion under a not",
                "feature",
                Without("priority"),
                "SPEC002",
                null,
                [new() { ["required"] = new JsonArray("priority", "id") }, []]
            },
            {
                "an applicator failing at the root, beside an assertion under a not",
                "feature",
                Valid(("priority", "urgent")),
                "SPEC002",
                null,
                [[], []]
            },
            { "an assertion under a then the document must satisfy", "feature", Valid(("value", 0)), "SPEC002", "value", [new() { ["minimum"] = 1 }] },
            {
                "a required key under a then, beside the allOf and then applicators and an assertion under a not",
                "feature",
                Without("synced_at"),
                "SPEC002",
                null,
                [[], [], new() { ["required"] = new JsonArray("synced_at") }, []]
            },
            {
                "an assertion under an if the document need not satisfy",
                "feature",
                Valid(("priority", "urgent"), ("scored_by", 5)),
                "SPEC002",
                "scored_by",
                [[]]
            },
            { "a not whose subschema passed, beside the allOf applicator", "feature", Valid(("kind", "legacy")), "SPEC002", null, [[], []] },
            { "an assertion in each failed anyOf branch", "feature", Valid(("mode", "c")), "SPEC002", "mode", [[], []] },
            { "an assertion in each failed oneOf branch", "feature", Valid(("choice", "y")), "SPEC002", "choice", [[], [], []] },
            { "a key the schema does not declare", "feature", Valid(("extra", 1)), "SPEC002", "extra", [[]] },
            {
                "an item's value outside its schema's enum",
                "task",
                new JsonObject { ["parent"] = "F1", ["id"] = "0001-01", ["priority"] = "urgent" },
                "SPEC003",
                "priority",
                [Enum("med")]
            },
            {
                "an epic's value other than its schema's const",
                "epic",
                new JsonObject { ["priority"] = "urgent" },
                "SPEC004",
                "priority",
                [new() { ["const"] = "med" }]
            },
        };

    /// <summary>Gets a document kind and the required keys of the schema the manifest names for it, in that schema's order.</summary>
    public static TheoryData<string, string, string[]> MissingFrontmatter =>
        new()
        {
            { "a Feature specification", "feature", ["priority", "id"] },
            { "an item", "task", ["parent", "id"] },
            { "an epic, whose schema requires no key", "epic", [] },
        };

    /// <summary>Gets a specification that breaks the manifest's section contract in one way.</summary>
    public static TheoryData<string, string> SectionFailures =>
        new()
        {
            { "a missing section", "## Goal\n\nText.\n\n## Design\n\nText.\n" },
            { "a section twice", "## Goal\n\nText.\n\n## Needs\n\nText.\n\n## Needs\n\nText.\n\n## Design\n\nText.\n" },
            { "sections out of order", "## Needs\n\nText.\n\n## Goal\n\nText.\n\n## Design\n\nText.\n" },
        };

    /// <summary>Gets a manifest, a specification whose table it rejects, and the section and headers that manifest gives.</summary>
    public static TheoryData<string, JsonObject, string, string, string[]> TableFailures =>
        new()
        {
            {
                "a matrix with another column",
                new JsonObject { ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Proof", "Status") } },
                "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| - | - | - | - |\n| B-001 | It | A test | Covered |\n",
                "9. Traceability Matrix",
                ["Claim ID", "Scenario", "Proof", "Status"]
            },
            {
                "a matrix with no table",
                new JsonObject { ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Proof", "Status") } },
                "## 9. Traceability Matrix\n\nNone.\n",
                "9. Traceability Matrix",
                ["Claim ID", "Scenario", "Proof", "Status"]
            },
            {
                "a matrix under the title the manifest's roles give it",
                new JsonObject
                {
                    ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Coverage", "12. Sign-off"),
                    ["roles"] = new JsonObject { ["matrix"] = "9. Coverage" },
                    ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Test", "Status") },
                },
                "## 9. Coverage\n\n| Claim ID | Scenario | Status |\n| - | - | - |\n| B-001 | It | Covered |\n",
                "9. Coverage",
                ["Claim ID", "Scenario", "Test", "Status"]
            },
            {
                "a claims table the manifest gives headers",
                new JsonObject { ["tables"] = new JsonObject { ["claims"] = new JsonArray("ID", "Claim", "Source", "Status") } },
                "## 3. Acceptance Criteria\n\n| ID | Claim | Status |\n| - | - | - |\n| B-001 | It does the thing. | Active |\n",
                "3. Acceptance Criteria",
                ["ID", "Claim", "Source", "Status"]
            },
        };

    [Theory]
    [MemberData(nameof(FrontmatterFailures))]
    public void AFrontmatterTheSchemaRejects_WhenItsDocumentIsMade_ShouldExpectTheSchemaTheKeyAndOnlyTheAssertionsThatFailedAtThatPlace(
        string because,
        string kind,
        JsonObject frontmatter,
        string ruleId,
        string? key,
        JsonObject[] constraints)
    {
        // Given
        var model = Model(kind, frontmatter);

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [new FrontmatterSchemaRule()]));

        // Then
        var violations = document.Violations.Where(violation => violation.RuleId == ruleId && violation.Identifier == key).ToList();
        violations.Should().AllSatisfy(violation =>
        {
            (violation.Expected["schema"]?.GetValue<string>()).Should().Be(SchemaPaths[kind], because);
            violation.Expected.ContainsKey("key").Should().Be(key is not null, because);
            (violation.Expected["key"]?.GetValue<string>()).Should().Be(key, because);
        });
        violations.Select(static violation => Canonical(violation.Expected["constraints"])).Should()
            .BeEquivalentTo(constraints.Select(static expected => Canonical(expected)), because);
    }

    [Theory]
    [MemberData(nameof(MissingFrontmatter))]
    public void ADocumentWithNoFrontmatter_WhenItsDocumentIsMade_ShouldExpectItsKindsSchemaAndThatSchemasRequiredKeysInItsOrder(
        string because,
        string kind,
        string[] required)
    {
        // Given
        var model = Model(kind, null);

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [new FrontmatterSchemaRule()]));

        // Then
        var expected = document.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC001", because).Which.Expected;
        (expected["schema"]?.GetValue<string>()).Should().Be(SchemaPaths[kind], because);
        JsonNode.DeepEquals(expected["required"], new JsonArray([.. required.Select(static name => JsonValue.Create(name))])).Should().BeTrue(
            "{0}: required should be [{1}], but expected is {2}", because, string.Join(", ", required), expected.ToJsonString());
    }

    [Theory]
    [MemberData(nameof(SectionFailures))]
    public void ASectionViolation_WhenItsDocumentIsMade_ShouldExpectTheManifestsSectionTitlesInOrder(string because, string text)
    {
        // Given
        SpecModel model = new SpecModelFixture()
            .WithManifest(new JsonObject
            {
                ["sections"] = new JsonArray("Goal", "Needs", "Design"),
                ["roles"] = new JsonObject { ["claims"] = "Goal", ["matrix"] = "Needs", ["signOff"] = "Design" },
            })
            .WithFeatures(Feature(text));

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [new SectionStructureRule()]));

        // Then
        document.Violations.Where(static violation => violation.RuleId == "SPEC010").Should().NotBeEmpty(because)
            .And.AllSatisfy(violation => JsonNode.DeepEquals(violation.Expected["sections"], new JsonArray("Goal", "Needs", "Design")).Should().BeTrue(
                "{0}: sections should be the manifest's, in its order, but expected is {1}", because, violation.Expected.ToJsonString()));
    }

    [Theory]
    [MemberData(nameof(TableFailures))]
    public void ATableViolation_WhenItsDocumentIsMade_ShouldExpectTheSectionTitleAndTheHeadersTheManifestGivesItInOrder(
        string because,
        JsonObject manifest,
        string text,
        string section,
        string[] headers)
    {
        // Given
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(Feature(text));

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [new SectionStructureRule()]));

        // Then
        var expected = document.Violations.Should()
            .ContainSingle(violation => violation.RuleId == "SPEC013" && violation.Identifier == section, because).Which.Expected;
        (expected["section"]?.GetValue<string>()).Should().Be(section, because);
        JsonNode.DeepEquals(expected["headers"], new JsonArray([.. headers.Select(static header => JsonValue.Create(header))])).Should().BeTrue(
            "{0}: headers should be [{1}], but expected is {2}", because, string.Join(", ", headers), expected.ToJsonString());
    }

    private static JsonObject Enum(params string[] values) => new() { ["enum"] = new JsonArray([.. values.Select(static value => JsonValue.Create(value))]) };

    private static string Canonical(JsonNode? node) =>
        node switch
        {
            JsonObject members => "{" + string.Join(",", members.OrderBy(static member => member.Key, StringComparer.Ordinal)
                .Select(static member => JsonSerializer.Serialize(member.Key) + ":" + Canonical(member.Value))) + "}",
            JsonArray elements => "[" + string.Join(",", elements.Select(Canonical)) + "]",
            null => "null",
            _ => node.ToJsonString(),
        };

    private static JsonObject Valid(params (string Key, JsonNode? Value)[] overrides)
    {
        var frontmatter = new JsonObject
        {
            ["id"] = "F1",
            ["priority"] = "low",
            ["value"] = 1,
            ["scored_by"] = "someone",
            ["synced_at"] = "2026-10-07T00:00:00Z",
        };

        foreach (var (key, value) in overrides)
        {
            frontmatter[key] = value;
        }

        return frontmatter;
    }

    private static JsonObject Without(string key)
    {
        var frontmatter = Valid();
        frontmatter.Remove(key);
        return frontmatter;
    }

    private static SpecModel Model(string kind, JsonObject? frontmatter)
    {
        Frontmatter node = new FrontmatterFixture().WithNode(frontmatter);
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", SpecTree.ManifestPath)] = new(
                """
                {
                  "identifiers": { "epic": "^[0-9]{4}$" },
                  "frontmatterSchemas": { "feature": "spec.json", "task": "item.json", "epic": "group.json" }
                }
                """),
            [Path.Combine("repo", SchemaPaths["feature"])] = new(FeatureSchema),
            [Path.Combine("repo", SchemaPaths["task"])] = new(ItemSchema),
            [Path.Combine("repo", SchemaPaths["epic"])] = new(EpicSchema),
        });
        var fixture = new SpecModelFixture().WithSchemas(EngineServices.LoadOnDiskSchemas(files, "repo"));

        return kind switch
        {
            "feature" => fixture.WithFeatures(new FeatureSpecFixture().WithDocument(new SpecDocumentFixture().WithFrontmatter(node))),
            "task" => fixture.WithItems(new ChildItemFixture().WithFrontmatter(node)),
            _ => fixture.WithList([(EpicFile)new EpicFileFixture().WithFrontmatter(node)]),
        };
    }

    private static FeatureSpec Feature(string text) =>
        new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath("src/sample/.spec/README.md"))
            .WithDocument(SpecDocument.Parse(text, "src/sample/.spec/README.md"));

    private const string FeatureSchema =
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
            "choice": { "oneOf": [{ "const": "x" }, { "type": "integer" }] }
          },
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

    private const string EpicSchema =
        """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "properties": { "priority": { "const": "med" } }
        }
        """;

    private static readonly Dictionary<string, string> SchemaPaths = new(StringComparer.Ordinal)
    {
        ["feature"] = SpecTree.FrontmatterSchemaFolder + "/spec.json",
        ["task"] = SpecTree.FrontmatterSchemaFolder + "/item.json",
        ["epic"] = SpecTree.FrontmatterSchemaFolder + "/group.json",
    };
}
