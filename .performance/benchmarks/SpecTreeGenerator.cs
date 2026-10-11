using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Benchmarks;

/// <summary>Writes the clean specification tree a benchmark measures over (<c>0109-F1</c> B-015, B-016; decision 0004).</summary>
public static class SpecTreeGenerator
{
    /// <summary>
    /// Writes the shipped manifest and frontmatter schemas, then <paramref name="specifications"/> co-located Feature
    /// specifications, each with its companion Gherkin file, under <paramref name="root"/>.
    /// </summary>
    /// <param name="root">An existing, empty directory, which the caller creates and deletes.</param>
    /// <param name="specifications">How many specifications the tree holds.</param>
    public static void Generate(string root, int specifications)
    {
        var manifest = Path.Combine(root, SpecManifest.RelativePath);
        var schema = Directory.CreateDirectory(Path.GetDirectoryName(manifest)!).FullName;

        using (var shipped = typeof(SpecManifest).Assembly.GetManifestResourceStream(DefaultManifest)!)
        using (var file = File.Create(manifest))
        {
            shipped.CopyTo(file);
        }

        var structure = SpecManifest.Load(new FileSystem(), root);
        var version = SchemaVersions.Embedded.Select(structure.SchemaVersion);

        Write(Path.Combine(schema, structure.FrontmatterSchemas["feature"]), version.FeatureSchema);
        Write(Path.Combine(schema, structure.FrontmatterSchemas["task"]), version.ItemSchema);
        Write(Path.Combine(schema, structure.FrontmatterSchemas["epic"]), version.EpicSchema);

        for (var index = 1; index <= specifications; index++)
        {
            var number = index.ToString("D4", CultureInfo.InvariantCulture);
            var directory = Directory.CreateDirectory(Path.Combine(root, "features", $"feature-{number}", ".spec")).FullName;

            Write(Path.Combine(directory, "README.md"), Specification(index, number));
            Write(Path.Combine(directory, $"feature-{number}.feature"), Scenarios(number));
        }
    }

    private static string Specification(int index, string number)
    {
        var claims = Enumerable.Range(1, Claims).Select(Claim).ToList();

        string[] lines =
        [
            "---",
            $"title: \"Specification: Generated feature {number}\"",
            $"description: \"Specification {number} of a generated benchmark tree\"",
            "type: feature",
            $"id: \"F{index.ToString(CultureInfo.InvariantCulture)}\"",
            $"epic: \"{Epic}\"",
            "spec_status: approved",
            "status: done",
            "priority: med",
            "value: 0",
            "risk: 0",
            "rank: 0",
            "scored_by: null",
            "scored_on: null",
            "domain: \"Benchmarking\"",
            "author: \"spec-author\"",
            "milestone: null",
            "children: []",
            "depends_on: []",
            "blocks: []",
            "spikes: []",
            $"created: \"{Date}\"",
            $"updated: \"{Date}\"",
            "github_issue: null",
            "synced_at: null",
            "---",
            string.Empty,
            $"# Specification: Generated feature {number}",
            string.Empty,
            "## 1. Business Goal",
            string.Empty,
            $"Generated feature {number} removes one failure state for the operator.",
            string.Empty,
            "## 2. User Needs",
            string.Empty,
            "| # | Persona | Need | Pain Point Today |",
            "| - | ------- | ---- | ---------------- |",
            "| 1 | Operator | An output for every input | Has none |",
            string.Empty,
            "## 3. Acceptance Criteria",
            string.Empty,
            "| ID | Claim | Source | Status |",
            "| -- | ----- | ------ | ------ |",
            .. claims.Select(static claim => $"| B-{claim} | Given input {claim}, this Feature produces output {claim}. | generated | Active |"),
            string.Empty,
            "## 4. Constraints",
            string.Empty,
            "| ID | Constraint | Rules Out |",
            "| -- | ---------- | --------- |",
            "| C-1 | An output is a function of its input alone. | An output that reads a clock |",
            string.Empty,
            "## 5. Out of Scope",
            string.Empty,
            "| # | Item | Exclusion Reason |",
            "| - | ---- | ---------------- |",
            "| 1 | Any other input | Owned by a sibling Feature |",
            string.Empty,
            "## 6. Concern Separation",
            string.Empty,
            "| # | Concern | Classification |",
            "| - | ------- | -------------- |",
            "| 1 | Producing an output | Business |",
            string.Empty,
            "## 7. Technical Design",
            string.Empty,
            "One function maps an input to its output.",
            string.Empty,
            "## 8. Testing Strategy",
            string.Empty,
            "Each claim has one scenario and one test.",
            string.Empty,
            "## 9. Traceability Matrix",
            string.Empty,
            "| Claim ID | Scenario | Test | Status |",
            "| -------- | -------- | ---- | ------ |",
            .. claims.Select(static claim => $"| B-{claim} | Input {claim} produces output {claim} | `GeneratedTests.Input{claim}` | Covered |"),
            string.Empty,
            "## 10. Lessons / Spec Deltas",
            string.Empty,
            "None.",
            string.Empty,
            "## 11. Open Questions",
            string.Empty,
            "None.",
            string.Empty,
            "## 12. Sign-off",
            string.Empty,
            "| Section | Status | Reviewer | Note |",
            "| ------- | ------ | -------- | ---- |",
            "| 1-5 | \U0001F7E2 | spec-reviewer | Approved. |",
            string.Empty,
            "## Tasks",
            string.Empty,
            "None.",
            string.Empty,
            "## Scoring",
            string.Empty,
            "| Field | Value | Basis |",
            "| ----- | ----- | ----- |",
            "| value | 0 | Not yet scored |",
            "| risk | 0 | Not yet scored |",
            string.Empty,
        ];

        return string.Join('\n', lines);
    }

    private static string Scenarios(string number)
    {
        string[] lines =
        [
            $"Feature: Generated feature {number}",
            .. Enumerable.Range(1, Claims).Select(Claim).SelectMany(static claim => new[]
            {
                string.Empty,
                $"  @B-{claim}",
                $"  Scenario: Input {claim} produces output {claim}",
                $"    Given input {claim}",
                "    When this Feature runs",
                $"    Then it produces output {claim}",
            }),
            string.Empty,
        ];

        return string.Join('\n', lines);
    }

    private static string Claim(int claim) => claim.ToString("D3", CultureInfo.InvariantCulture);

    private static void Write(string path, string text) => File.WriteAllText(path, text, Utf8);

    private const int Claims = 3;
    private const string Epic = "0001";
    private const string Date = "2026-01-01";
    private const string DefaultManifest = "specht.default-manifest.json";

    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
}
