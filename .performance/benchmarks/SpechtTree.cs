using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Benchmarks;

/// <summary>The clean specification tree a benchmark measures over, held in memory (<c>0109-F1</c> B-015, B-016; decision 0004).</summary>
public sealed class SpechtTree
{
    private SpechtTree(IReadOnlyList<SpechtTreeFile> files) => Files = files;

    /// <summary>
    /// Gets the tree's files: the manifest, the three frontmatter schemas, then each specification and its Gherkin file, in
    /// index order.
    /// </summary>
    public IReadOnlyList<SpechtTreeFile> Files { get; }

    /// <summary>Builds the tree that holds <paramref name="specifications"/> specifications.</summary>
    /// <param name="specifications">How many specifications the tree holds.</param>
    /// <returns>A new tree, holding the same files with the same contents for the same size.</returns>
    public static SpechtTree Of(int specifications)
    {
        var manifest = ShippedManifest();
        var keys = JsonNode.Parse(manifest)!;
        var names = keys["frontmatterSchemas"]!;
        var version = SchemaVersions.Embedded.Select(
            SemanticVersion.TryParse(keys["schemaVersion"]?.GetValue<string>(), out var pinned) ? pinned : new SemanticVersion(0, 1, 0));
        var schema = SpecManifest.RelativePath[..(SpecManifest.RelativePath.LastIndexOf('/') + 1)];

        List<SpechtTreeFile> files =
        [
            new(SpecManifest.RelativePath, manifest),
            Text(schema + names["feature"]!.GetValue<string>(), version.FeatureSchema),
            Text(schema + names["task"]!.GetValue<string>(), version.ItemSchema),
            Text(schema + names["epic"]!.GetValue<string>(), version.EpicSchema),
        ];

        for (var index = 1; index <= specifications; index++)
        {
            var number = index.ToString("D4", CultureInfo.InvariantCulture);
            var directory = $"features/feature-{number}/.spec/";

            files.Add(Text(directory + "README.md", Specification(index, number)));
            files.Add(Text(directory + $"feature-{number}.feature", Scenarios(number)));
        }

        return new SpechtTree(files);
    }

    private static byte[] ShippedManifest()
    {
        using var shipped = typeof(SpecManifest).Assembly.GetManifestResourceStream(DefaultManifest)!;
        using var bytes = new MemoryStream();

        shipped.CopyTo(bytes);

        return bytes.ToArray();
    }

    private static SpechtTreeFile Text(string relativePath, string text) => new(relativePath, Utf8.GetBytes(text));

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

    private const int Claims = 3;
    private const string Epic = "0001";
    private const string Date = "2026-01-01";
    private const string DefaultManifest = "specht.default-manifest.json";

    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
}
