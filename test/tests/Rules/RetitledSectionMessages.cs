using System.Text.Json.Nodes;

namespace Specht.Tests.Rules;

/// <summary>
/// Every message that cites a section, under a manifest that retitles the claims, matrix and sign-off roles (<c>0001-F5</c>
/// B-009; decision 0019): the rule id, the manifest, the specification at <c>src/sample/.spec/README.md</c>, its companion
/// beside it or none, the reference the one finding names and the default reference it no longer names. The numbered
/// titles <c>4. Claims</c>, <c>7. Matrix</c> and <c>11. Approvals</c> are cited <c>§ 4</c>, <c>§ 7</c> and <c>§ 11</c>, at
/// schema 0.3.0; the unnumbered titles <c>Claims</c>, <c>Matrix</c> and <c>Approvals</c> are cited <c>## Claims</c>,
/// <c>## Matrix</c> and <c>## Approvals</c>, at the oldest pin each rule ships at (0.1.0, 0.3.0 for SPEC022, SPEC032 and
/// SPEC035, or 0.4.0 for SPEC023 and SPEC033).
/// </summary>
public sealed class RetitledSectionMessages : TheoryData<string, string, string, string?, string, string>
{
    public RetitledSectionMessages()
    {
        AddSites(new("4. Claims", "7. Matrix", "11. Approvals", "§ 4", "§ 7", "§ 11", "0.3.0"));
        AddSites(new("Claims", "Matrix", "Approvals", "## Claims", "## Matrix", "## Approvals", null));
    }

    private static string Row(string claim, string scenario, string test, string status) => $"| {claim} | {scenario} | {test} | {status} |\n";

    private void AddSites(Titles titles)
    {
        var matrixRow = Row("B-001", "It holds", "`AlphaTest`", "Covered");
        Add(
            "SPEC021",
            titles.Manifest(titles.Pin),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(matrixRow),
            Companion + "\n  @B-404\n  Scenario: Phantom\n    Given nothing\n",
            titles.ClaimsReference,
            "§ 3");
        Add("SPEC030", titles.Manifest(titles.Pin), Frontmatter + titles.ClaimTable("B-001", "B-001"), null, titles.ClaimsReference, "§ 3");

        var unrowed = Frontmatter + titles.ClaimTable("B-001", "B-002") + titles.MatrixTable(matrixRow);
        Add("SPEC031", titles.Manifest(titles.Pin), unrowed, null, titles.MatrixReference, "§ 9");
        Add("SPEC031", titles.Manifest(titles.Pin), unrowed, null, titles.ClaimsReference, "§ 3");
        Add(
            "SPEC031",
            titles.Manifest(titles.Pin),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(matrixRow, matrixRow),
            null,
            titles.MatrixReference,
            "§ 9");
        var unclaimed = Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(matrixRow, Row("B-009", "It holds", "`AlphaTest`", "Covered"));
        Add("SPEC031", titles.Manifest(titles.Pin), unclaimed, null, titles.MatrixReference, "§ 9");
        Add("SPEC031", titles.Manifest(titles.Pin), unclaimed, null, titles.ClaimsReference, "§ 3");

        Add(
            "SPEC035",
            titles.Manifest("0.3.0"),
            Frontmatter + titles.ClaimTable(("B-001", "Withdrawn")) + titles.MatrixTable(matrixRow),
            null,
            titles.MatrixReference,
            "§ 9");

        var titleMatching = new JsonObject { ["matrix"] = new JsonObject { ["scenarioTitles"] = true } };
        Add(
            "SPEC022",
            titles.Manifest("0.3.0", titleMatching),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(Row("B-001", "No such case", "`AlphaTest`", "Covered")),
            Companion,
            titles.MatrixReference,
            "§ 9");
        Add(
            "SPEC022",
            titles.Manifest("0.3.0", titleMatching),
            Frontmatter + titles.ClaimTable("B-001", "B-002") + titles.MatrixTable(Row("B-001", "Other case", "`AlphaTest`", "Covered")),
            Companion + "\n  @B-002\n  Scenario: Other case\n    Given another thing\n",
            titles.MatrixReference,
            "§ 9");

        Add(
            "SPEC023",
            titles.Manifest("0.4.0"),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(matrixRow),
            null,
            titles.MatrixReference,
            "§ 9");

        Add(
            "SPEC033",
            titles.Manifest("0.4.0"),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(Row("B-009", "It holds", "`AlphaTest`", "Covered")),
            null,
            titles.MatrixReference,
            "§ 9");

        var tests = new JsonObject { ["tests"] = new JsonObject { ["files"] = new JsonArray("test/**/*.cs") } };
        Add(
            "SPEC032",
            titles.Manifest("0.3.0", tests),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(Row("B-001", "It holds", "`NoSuchTest`", "Covered")),
            null,
            titles.MatrixReference,
            "§ 9");
        Add(
            "SPEC032",
            titles.Manifest("0.3.0", tests),
            Frontmatter + titles.ClaimTable("B-001") + titles.MatrixTable(Row("B-001", "It holds", "NoSuchTest", "Covered")),
            null,
            titles.MatrixReference,
            "§ 9");

        Add(
            "SPEC060",
            titles.Manifest(titles.Pin),
            Approved + titles.MatrixTable(Row("B-001", "It holds", "Missing", "Missing")),
            null,
            titles.MatrixReference,
            "§ 9");
        Add("SPEC061", titles.Manifest(titles.Pin), Approved + titles.SignOffTable(), null, titles.SignOffReference, "§ 12");
    }

    /// <summary>The three role titles, the reference a message gives each and the pin the 0.1.0 rules are read at.</summary>
    private sealed record Titles(
        string Claims,
        string Matrix,
        string SignOff,
        string ClaimsReference,
        string MatrixReference,
        string SignOffReference,
        string? Pin)
    {
        public string Manifest(string? pin, JsonObject? settings = null)
        {
            var manifest = new JsonObject
            {
                ["sections"] = new JsonArray(Claims, Matrix, SignOff),
                ["roles"] = new JsonObject { ["claims"] = Claims, ["matrix"] = Matrix, ["signOff"] = SignOff },
            };
            if (pin is not null)
            {
                manifest["schemaVersion"] = pin;
            }

            foreach (var (key, value) in settings ?? [])
            {
                manifest[key] = value?.DeepClone();
            }

            return manifest.ToJsonString();
        }

        public string ClaimTable(params string[] claims) => ClaimTable([.. claims.Select(static claim => (claim, "Active"))]);

        public string ClaimTable(params (string Claim, string Status)[] claims) =>
            $"## {Claims}\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                + string.Concat(claims.Select(static claim => $"| {claim.Claim} | It holds. | brd | {claim.Status} |\n"))
                + "\n";

        public string MatrixTable(params string[] rows) =>
            $"## {Matrix}\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n" + string.Concat(rows) + "\n";

        public string SignOffTable() =>
            $"## {SignOff}\n\n| Section | Status | Reviewer | Note |\n| ------- | ------ | -------- | ---- |\n| 1-5 | \U0001F7E1 | spec-reviewer | Draft |\n";
    }

    private const string Frontmatter = "---\nepic: \"0001\"\nid: F1\n---\n\n";

    private const string Approved = "---\nepic: \"0001\"\nid: F1\nspec_status: approved\nstatus: done\n---\n\n";

    private const string Companion = "Feature: it\n\n  @B-001\n  Scenario: It holds\n    Given a thing\n";
}
