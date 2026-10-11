using Specht.Tests.Shared;

namespace Specht.Tests.Engine.Baseline;

/// <summary>
/// Writes the baseline tree (<c>0001-F1</c> B-004, C-10, decision 0004) into a <see cref="SpecTree"/>: every rule,
/// <c>SPEC001</c> to <c>SPEC061</c>, broken in each layout it applies to - the legacy layout under epic
/// <see cref="LegacyEpic"/>, the co-located layout under epic <see cref="CoLocatedEpic"/>. Its content is fixed, so the
/// same tree is written every time; <c>Baseline/engine-e7dba24.json</c> is the engine's verdict on it at commit
/// <c>e7dba24</c>.
/// </summary>
public static class BaselineTree
{
    /// <summary>The epic every legacy specification in the tree declares, under <c>epics/</c>.</summary>
    public const string LegacyEpic = "0001";

    /// <summary>The epic every co-located specification in the tree declares, under <c>src/</c>.</summary>
    public const string CoLocatedEpic = "0002";

    /// <summary>Writes the baseline tree under <paramref name="tree"/>'s root.</summary>
    /// <param name="tree">A tree holding only the schema set.</param>
    public static void Write(SpecTree tree)
    {
        Break(new Layout(tree, LegacyEpic, Legacy: true));
        Break(new Layout(tree, CoLocatedEpic, Legacy: false));

        tree.WriteFeature(LegacyEpic, "F40");
        tree.WriteCoLocatedFeature("src/moved-without-removing", LegacyEpic, "F40");
        tree.WriteFeature(LegacyEpic, "F41", Frontmatter("epic", "\"0003\""));
        tree.WriteFeature(LegacyEpic, "F42", Frontmatter("id", "\"F420\""));
        tree.WriteEpic(LegacyEpic, Frontmatter("priority", "urgent"));
    }

    private static void Break(Layout layout)
    {
        var epic = layout.Epic;

        StripFrontmatter(layout.Spec("F1", "no-frontmatter"));
        layout.Spec("F2", "status-outside-enum", Frontmatter("spec_status", "nearly"));
        var item = layout.Spec("F3", "item-status-outside-enum", Frontmatter("children", $"[\"{epic}-01\"]"));
        layout.Item(item, "01", "F3", frontmatter: Frontmatter("status", "nearly"));

        layout.Spec("F4", "section-missing", sections: [.. SpecTree.Sections.Where(static section => !section.StartsWith("## 8. ", Ordinal))]);
        layout.Spec("F5", "section-twice", sections: [.. SpecTree.Sections, "## 7. Technical Design\n\nAgain.\n"]);
        layout.Spec("F6", "sections-out-of-order", sections: Swapped(5, 6));
        layout.Spec("F7", "matrix-without-table", sections: SpecTree.SectionsWith(Matrix, $"## {Matrix}\n\nNone yet.\n"));
        layout.Spec("F8", "matrix-wrong-headers", sections: SpecTree.SectionsWith(Matrix, ThreeColumnMatrix));

        DeleteFeatureFile(layout.Spec("F9", "no-feature-file"));
        AddFeatureFile(layout.Spec("F10", "two-feature-files"));
        layout.Tree.WriteFeatureFile(layout.Spec("F11", "tag-without-claim"), GhostScenario);

        layout.Spec("F12", "claim-id-grammar", sections: SpecTree.SectionsWith(Claims, ClaimRows(Claim("B-001"), Claim("B-1"))));
        layout.Spec("F13", "claim-twice", sections: SpecTree.SectionsWith(Claims, ClaimRows(Claim("B-001"), Claim("B-001"))));
        layout.Spec("F14", "claim-without-row", sections: SpecTree.SectionsWith(Claims, ClaimRows(Claim("B-001"), Claim("B-002"))));
        layout.Spec("F15", "claim-with-two-rows", sections: SpecTree.SectionsWith(Matrix, MatrixRows(Row("B-001"), Row("B-001"))));
        layout.Spec("F16", "row-without-claim", sections: SpecTree.SectionsWith(Matrix, MatrixRows(Row("B-001"), Row("B-999"))));

        layout.Spec("F17", "child-without-file", Frontmatter("children", $"[\"{epic}-97\"]"));
        var twice = layout.Spec("F18", "child-in-two-files", Frontmatter("children", $"[\"{epic}-05\"]"));
        layout.Item(twice, "05", "F18", slug: "first");
        layout.Item(twice, "05", "F18", slug: "second");
        layout.Spec("F19", "spike-without-file", Frontmatter("spikes", $"[\"{epic}-98\"]"));
        var spike = layout.Spec("F20", "spike-of-another-type", Frontmatter("spikes", $"[\"{epic}-06\"]"));
        layout.Item(spike, "06", "F20", type: "task");

        var renamed = layout.Spec("F21", "item-id-not-its-file", Frontmatter("children", $"[\"{epic}-03\"]"));
        layout.Item(renamed, "03", "F21", idOverride: $"{epic}-02");
        var adopted = layout.Spec("F22", "item-parent-elsewhere", Frontmatter("children", $"[\"{epic}-04\"]"));
        layout.Item(adopted, "04", "F99");
        var first = layout.Spec("F23", "item-id-reused-first", Frontmatter("children", $"[\"{epic}-07\"]"));
        var second = layout.Spec("F24", "item-id-reused-second", Frontmatter("children", $"[\"{epic}-07\"]"));
        layout.Item(first, "07", "F23");
        layout.Item(second, "07", "F24");

        layout.Spec("F25", "depends-on-nothing", Frontmatter("depends_on", "[\"F99\"]"));
        layout.Spec("F26", "blocks-one-sided", Frontmatter("blocks", "[\"F2\"]"));
        layout.Spec("F27", "depends-on-itself", Frontmatter("depends_on", "[\"F27\"]"));
        layout.Spec("F28", "cycle-first", new Dictionary<string, string> { ["depends_on"] = "[\"F29\"]", ["blocks"] = "[\"F29\"]" });
        layout.Spec("F29", "cycle-second", new Dictionary<string, string> { ["depends_on"] = "[\"F28\"]", ["blocks"] = "[\"F28\"]" });

        layout.Spec("F30", "approved-with-missing", Frontmatter("spec_status", "approved"), SignedOff(SpecTree.Sections));
        layout.Spec("F31", "approved-in-draft", Frontmatter("spec_status", "approved"), SpecTree.SectionsWith(Matrix, MatrixRows(Proven)));
    }

    private static Dictionary<string, string> Frontmatter(string key, string value) => new(StringComparer.Ordinal) { [key] = value };

    private static List<string> Swapped(int first, int second)
    {
        var sections = SpecTree.Sections.ToList();
        (sections[first], sections[second]) = (sections[second], sections[first]);

        return sections;
    }

    private static string Claim(string id) => $"| {id} | It does the thing. | brd | Active |\n";

    private static string ClaimRows(params string[] rows) =>
        $"## {Claims}\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n" + string.Concat(rows);

    private static string Row(string id) => $"| {id} | It does the thing | Missing | Missing |\n";

    private static string MatrixRows(params string[] rows) =>
        $"## {Matrix}\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n" + string.Concat(rows);

    private static List<string> SignedOff(IReadOnlyList<string> sections) =>
        [.. sections.Select(static section => section.StartsWith("## 12. ", Ordinal) ? SignOff : section)];

    private static void StripFrontmatter(string specPath)
    {
        var text = File.ReadAllText(specPath);
        File.WriteAllText(specPath, text[text.IndexOf("# Specification: ", Ordinal)..]);
    }

    private static void DeleteFeatureFile(string specPath) => File.Delete(Path.Combine(Path.GetDirectoryName(specPath)!, "feature.feature"));

    private static void AddFeatureFile(string specPath) =>
        File.WriteAllText(Path.Combine(Path.GetDirectoryName(specPath)!, "second.feature"), "Feature: another\n");

    private sealed record Layout(SpecTree Tree, string Epic, bool Legacy)
    {
        public string Spec(
            string id,
            string area,
            IReadOnlyDictionary<string, string>? frontmatter = null,
            IReadOnlyList<string>? sections = null)
        {
            if (Legacy)
            {
                return Tree.WriteFeature(Epic, id, frontmatter, sections);
            }

            var path = Tree.WriteCoLocatedFeature($"src/{area}", Epic, id, frontmatter);

            if (sections is not null)
            {
                var text = File.ReadAllText(path);
                var heading = $"# Specification: {id}\n\n";
                File.WriteAllText(path, text[..(text.IndexOf(heading, Ordinal) + heading.Length)] + string.Join('\n', sections));
            }

            return path;
        }

        public void Item(
            string specPath,
            string number,
            string parent,
            string type = "task",
            string? idOverride = null,
            string slug = "item",
            IReadOnlyDictionary<string, string>? frontmatter = null) =>
            Tree.WriteItem(specPath, $"{Epic}-{number}", $"{Epic}-{parent}", type, idOverride, slug, frontmatter);
    }

    private const StringComparison Ordinal = StringComparison.Ordinal;
    private const string Claims = "3. Acceptance Criteria";
    private const string Matrix = "9. Traceability Matrix";
    private const string Proven = "| B-001 | It does the thing | It.Unit.Tests.cs | Done |\n";

    private const string ThreeColumnMatrix =
        $"## {Matrix}\n\n| Claim ID | Scenario | Test |\n| -------- | -------- | ---- |\n| B-001 | It does the thing | Missing |\n";

    private const string GhostScenario =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-002\n  Scenario: A ghost\n    Given a ghost\n";

    private const string SignOff =
        "## 12. Sign-off\n\n| Section | Status | Reviewer | Note |\n| ------- | ------ | -------- | ---- |\n| 1-5 | \U0001F7E2 | spec-reviewer | Approved |\n";
}
