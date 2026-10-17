using System.Text.Json.Nodes;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>Synthetic branches for 0001-F3 B-016, B-017, B-018, B-027 and B-028.</summary>
public sealed class ItemsEdgesApprovalExpectations : TheoryData<string, SpecModel, ISpecRule, string, string, string?, JsonObject>
{
    public ItemsEdgesApprovalExpectations()
    {
        foreach (var (key, ruleId) in new[] { ("children", "SPEC040"), ("spikes", "SPEC041") })
        {
            FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                $"---\nepic: \"0001\"\nid: \"F1\"\n{key}: [\"0001-01\"]\n---\n", "src/sample/.spec/README.md"));
            foreach (var count in new[] { 0, 2 })
            {
                var items = Enumerable.Range(0, count).Select(static index => (ChildItem)new ChildItemFixture()
                    .WithFileName($"0001-01-{index}.md")
                    .WithRelativePath($"src/sample/.spec/0001-01-{index}.md")).ToList();
                var expected = new JsonObject { ["fileName"] = "0001-01-*.md" };
                if (key == "spikes")
                {
                    expected["type"] = "spike";
                }

                Add($"{key} resolves to {count} files", new SpecModelFixture().WithFeatures(feature).WithItems(items),
                    new ChildItemRule(), ruleId, count == 0 ? "was discovered" : "resolves to 2 files", null, expected);
            }

            if (key == "spikes")
            {
                ChildItem task = new ChildItemFixture().WithFrontmatter(new FrontmatterFixture()
                    .WithNode(new JsonObject { ["id"] = "0001-01", ["type"] = "task" }));
                Add("a declared spike is a task", new SpecModelFixture().WithFeatures(feature).WithItems(task),
                    new ChildItemRule(), ruleId, "not 'spike'", null, new JsonObject { ["fileName"] = "0001-01-*.md", ["type"] = "spike" });
            }
        }

        ChildItem first = new ChildItemFixture().WithRelativePath("src/z-first/.spec/0001-01-task.md")
            .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "0001-01" }));
        ChildItem duplicate = new ChildItemFixture().WithRelativePath("src/a-second/.spec/0001-01-task.md")
            .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "0001-01" }));
        SpecModel repeated = new SpecModelFixture().WithItems(first, duplicate);
        Add("first holder follows model order rather than path order", repeated, new ChildItemRule(), "SPEC044", "already used", null,
            new JsonObject { ["fileName"] = "0001-01-*.md", ["firstDeclaration"] = "src/z-first/.spec/0001-01-task.md" });
        Add("the existing duplicate-induced sequence finding is preserved", repeated, new ChildItemRule(), "SPEC044", "sequence skips 02", null,
            new JsonObject { ["fileName"] = "0001-02-*.md", ["nextNumber"] = 2 });
        ChildItem fourth = new ChildItemFixture().WithFileName("0001-04-task.md")
            .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "0001-04" }));
        ChildItem second = new ChildItemFixture().WithFileName("0001-02-task.md")
            .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "0001-02" }));
        Add("an interior gap names the first missing number", new SpecModelFixture().WithItems(fourth, first, second),
            new ChildItemRule(), "SPEC044", "sequence skips 03", null, new JsonObject { ["fileName"] = "0001-03-*.md", ["nextNumber"] = 3 });
        Add("a sequence must begin at one", new SpecModelFixture().WithItems(fourth), new ChildItemRule(), "SPEC044", "sequence skips 01", null,
            new JsonObject { ["fileName"] = "0001-01-*.md", ["nextNumber"] = 1 });

        foreach (var key in new[] { "depends_on", "blocks" })
        {
            FeatureSpec source = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                $"---\nepic: \"0001\"\nid: \"F1\"\n{key}: [\"0009/F9\"]\n---\n", "src/source/.spec/README.md"));
            FeatureSpec target = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "---\nepic: \"0002\"\nid: \"F2\"\n---\n", "src/target/.spec/README.md"));
            Add("unknown " + key + " gets sorted distinct qualified targets",
                new SpecModelFixture().WithFeatures(target, source, new FeatureSpecFixture().WithDocument(target.Document)),
                new DependencyRule(), "SPEC050", "not a discovered Feature", null, new JsonObject { ["features"] = new JsonArray("0001-F1", "0002-F2") });
            FeatureSpec linked = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                $"---\nepic: \"0001\"\nid: \"F1\"\n{key}: [\"0002/F2\"]\n---\n", "src/source/.spec/README.md"));
            Add("missing reciprocal " + key, new SpecModelFixture().WithFeatures(linked, target), new DependencyRule(), "SPEC051",
                "does not declare", null, new JsonObject
                {
                    ["edge"] = new JsonObject { ["feature"] = "0002-F2", ["key"] = key == "depends_on" ? "blocks" : "depends_on", ["value"] = "0001-F1" },
                });
            FeatureSpec self = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                $"---\nepic: \"0001\"\nid: \"F1\"\n{key}: [\"F1\"]\n---\n", "src/source/.spec/README.md"));
            Add("direct self " + key, new SpecModelFixture().WithFeatures(self), new DependencyRule(), "SPEC052", "this Feature itself", null,
            new JsonObject { ["cycle"] = new JsonArray("0001-F1", "0001-F1") });
            if (key == "depends_on")
            {
                Add("the existing self-cycle traversal finding remains", new SpecModelFixture().WithFeatures(self), new DependencyRule(),
                    "SPEC052", "dependency cycle:", null, new JsonObject { ["cycle"] = new JsonArray("0001-F1", "0001-F1") });
            }
        }

        foreach (var end in new[] { "F1", "F2" })
        {
            FeatureSpec lead = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "---\nepic: \"0001\"\nid: \"F1\"\ndepends_on: [\"F2\"]\n---\n", "src/lead/.spec/README.md"));
            FeatureSpec tail = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                $"---\nepic: \"0001\"\nid: \"F2\"\ndepends_on: [\"{end}\"]\n---\n", "src/tail/.spec/README.md"));
            Add("ordered cycle ending in " + end, new SpecModelFixture().WithFeatures(lead, tail), new DependencyRule(), "SPEC052",
                "dependency cycle: 0001-F1 ->", null, new JsonObject { ["cycle"] = new JsonArray("0001-F1", "0001-F2", "0001-" + end) });
        }

        foreach (var custom in new[] { false, true })
        {
            var matrix = custom ? "9. Coverage" : "9. Traceability Matrix";
            var signOff = custom ? "12. Approval" : "12. Sign-off";
            var missing = custom ? "TBD" : "Missing";
            var approved = custom ? "agreed" : "approved";
            var draft = custom ? "draft-marker" : "🟡";
            var blocked = custom ? "blocked-marker" : "🔴";
            var manifest = new JsonObject
            {
                ["sections"] = new JsonArray("3. Acceptance Criteria", matrix, signOff),
                ["roles"] = new JsonObject { ["matrix"] = matrix, ["signOff"] = signOff },
                ["markers"] = new JsonObject { ["missing"] = missing, ["approved"] = approved, ["draft"] = draft, ["blocked"] = blocked },
            };
            FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                $"---\nspec_status: {approved}\n---\n\n## {matrix}\n\n| ID | Test | Status |\n| - | - | - |\n"
                + $"| B-001 | Ready | Covered |\n| B-002 | {missing} | {missing} |\n\n## {signOff}\n\n| Section | Status |\n| - | - |\n"
                + $"| 1-5 | 🟢 |\n| 6-9 | {draft} |\n| 10-12 | text {blocked} text |\n", "src/sample/.spec/README.md"));
            SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);
            Add("all missing cells with custom names " + custom, model, new ApprovalRule(), "SPEC060", "§ 9", null,
            new JsonObject { ["section"] = matrix, ["row"] = 2, ["columns"] = new JsonArray(2, 3) });
            Add("draft sign-off row with custom names " + custom, model, new ApprovalRule(), "SPEC061", "§ 12", "6-9",
            new JsonObject { ["section"] = signOff, ["row"] = 2 });
            Add("blocked sign-off row with custom names " + custom, model, new ApprovalRule(), "SPEC061", "§ 12", "10-12",
            new JsonObject { ["section"] = signOff, ["row"] = 3 });
        }
    }
}
