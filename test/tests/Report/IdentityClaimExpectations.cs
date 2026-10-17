using System.Text.Json.Nodes;
using Specht.Discovery;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>Synthetic violations of every identity, companion, claim and matrix branch (0001-F3 B-012 to B-015).</summary>
public sealed class IdentityClaimExpectations : TheoryData<string, SpecModel, ISpecRule, string, string, JsonObject>
{
    public IdentityClaimExpectations()
    {
        foreach (var (key, epic, id, form, value) in new[]
                 {
                     ("epic", "0009", "F1", "^[0-9]{4}$", "0001"),
                     ("id", "0001", "F9", "^F[0-9]+[a-z]?$", "F1"),
                 })
        {
            var path = "epics/0001-epic/F1-feature/spec.md";
            FeatureSpec feature = new FeatureSpecFixture()
                .WithLocation(new SpecLocationFixture().WithRelativePath(path)
                    .WithLayout(new SpecLayout("epics", "epics/**/spec.md", new SpecPathIdentity(1, 2))))
                .WithDocument(SpecDocument.Parse($"---\nepic: \"{epic}\"\nid: \"{id}\"\n---\n", path));
            Add(key + " differs from its directory", new SpecModelFixture().WithFeatures(feature), new IdentityRule(),
                "SPEC011", feature.Identity, new JsonObject { ["form"] = form, ["value"] = value });
        }

        var hyphenPath = "epics/EP-01-epic/F1-feature/spec.md";
        FeatureSpec hyphenIdentity = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath(hyphenPath)
                .WithLayout(new SpecLayout("epics", "epics/**/spec.md", new SpecPathIdentity(1, 2))))
            .WithDocument(SpecDocument.Parse("---\nepic: \"EP-09\"\nid: \"F1\"\n---\n", hyphenPath));
        Add("a configured identity grammar can contain a hyphen", new SpecModelFixture()
                .WithManifest(new JsonObject { ["identifiers"] = new JsonObject { ["epic"] = "^EP-[0-9]{2}$" } })
                .WithFeatures(hyphenIdentity), new IdentityRule(), "SPEC011", hyphenIdentity.Identity,
            new JsonObject { ["form"] = "^EP-[0-9]{2}$", ["value"] = "EP-01" });

        FeatureSpec first = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath("src/z-first/.spec/README.md"))
            .WithDocument(SpecDocument.Parse("---\nepic: \"0001\"\nid: \"F1\"\n---\n", "src/z-first/.spec/README.md"));
        FeatureSpec second = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath("src/a-second/.spec/README.md"))
            .WithDocument(SpecDocument.Parse("---\nepic: \"0001\"\nid: \"F1\"\n---\n", "src/a-second/.spec/README.md"));
        Add("the first identity follows model order, not path order", new SpecModelFixture().WithFeatures(first, second),
            new IdentityRule(), "SPEC012", "0001-F1", new JsonObject
            {
                ["form"] = "{epic}-{feature}",
                ["value"] = "0001-F1",
                ["firstDeclaration"] = "src/z-first/.spec/README.md",
            });

        foreach (var key in new[] { "id", "parent" })
        {
            FeatureSpec parent = new FeatureSpecFixture()
                .WithDocument(SpecDocument.Parse("---\nepic: \"0001\"\nid: \"F1\"\n---\n", "src/sample/.spec/README.md"));
            ChildItem item = new ChildItemFixture().WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject
            {
                ["id"] = key == "id" ? "0001-02" : "0001-01",
                ["parent"] = key == "parent" ? "0009-F9" : "0001-F1",
            }));
            Add("item " + key + " differs from its source", new SpecModelFixture().WithFeatures(parent).WithItems(item),
                new ChildItemRule(), "SPEC043", item.Id!, new JsonObject
                {
                    ["form"] = key == "id" ? "^[0-9]{4}-[0-9]{2}$" : "{epic}-{feature}",
                    ["value"] = key == "id" ? "0001-01" : "0001-F1",
                });
        }

        foreach (var count in new[] { 0, 2 })
        {
            FeatureSpec feature = new FeatureSpecFixture()
                .WithDocument(SpecDocument.Parse("---\nepic: \"0001\"\nid: \"F1\"\n---\n", "src/sample/.spec/README.md"))
                .WithFeatureFiles(count == 0 ? [] : [new FeatureFileFixture(), new FeatureFileFixture()]);
            Add($"{count} companions", new SpecModelFixture().WithFeatures(feature), new FeatureFileRule(), "SPEC020", "0001-F1",
                new JsonObject { ["companionCount"] = 1 });
        }

        FeatureSpec companion = new FeatureSpecFixture()
            .WithDocument(SpecDocument.Parse(Claims + "| B-002 | Duplicate. |\n| BAD | Invalid. |\n", "src/sample/.spec/README.md"))
            .WithFeatureFiles(new FeatureFileFixture().WithTags(new FeatureTagParser().Parse("@B-404")));
        Add("unresolved tag gets distinct valid ids in declaration order", new SpecModelFixture().WithFeatures(companion),
            new FeatureFileRule(), "SPEC021", "B-404", new JsonObject { ["claims"] = new JsonArray("B-002", "B-001") });

        foreach (var (grammar, claim) in new[] { ("^B-[0-9]{3}[a-z]?$", "B-12"), ("^REQ[0-9]{2}$", "B-001") })
        {
            FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| " + claim + " | Wrong form. |\n",
                "src/sample/.spec/README.md"));
            SpecModel model = new SpecModelFixture().WithManifest(new JsonObject
            { ["identifiers"] = new JsonObject { ["claim"] = grammar } }).WithFeatures(feature);
            Add("claim rejected by " + grammar, model, new ClaimRule(), "SPEC030", claim, new JsonObject { ["grammar"] = grammar });
        }

        FeatureSpec duplicate = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
            Claims + "| B-001 | Duplicate. |\n", "src/sample/.spec/README.md"));
        Add("duplicate claim names the original row", new SpecModelFixture().WithFeatures(duplicate), new ClaimRule(), "SPEC030", "B-001",
            new JsonObject { ["grammar"] = "^B-[0-9]{3}[a-z]?$", ["firstDeclarationLine"] = 6 });

        foreach (var (defect, rows, id, counts) in new[]
                 {
                     ("missing", "| B-001 | Test. |\n", "B-002", new JsonObject { ["B-002"] = 0, ["B-001"] = 1 }),
                     ("duplicate", "| B-001 | Test. |\n| B-002 | Test. |\n| B-002 | Again. |\n", "B-002",
                         new JsonObject { ["B-002"] = 2, ["B-001"] = 1 }),
                     ("undeclared", "| B-001 | Test. |\n| B-002 | Test. |\n| B-404 | Extra. |\n", "B-404",
                         new JsonObject { ["B-002"] = 1, ["B-001"] = 1 }),
                 })
        {
            FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpecDocument.Parse(
                Claims + "\n## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n" + rows,
                "src/sample/.spec/README.md"));
            Add(defect + " matrix row", new SpecModelFixture().WithFeatures(feature), new ClaimRule(), "SPEC031", id,
                new JsonObject { ["requiredRows"] = 1, ["rowsByClaim"] = counts });
        }
    }

    private const string Claims =
        "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-002 | Second. |\n| B-001 | First. |\n";
}
