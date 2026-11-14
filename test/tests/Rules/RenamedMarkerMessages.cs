namespace Specht.Tests.Rules;

/// <summary>
/// Every message that names a marker, under a manifest that renames it under <c>markers</c> (<c>0001-F5</c> B-009, B-003;
/// decision 0019): the rule id, the manifest, the specification at <c>src/sample/.spec/README.md</c>, the text the one
/// finding names and the default text it no longer names. SPEC060 selects by the done marker from schema 0.3.0 and by the
/// approved marker below it.
/// </summary>
public sealed class RenamedMarkerMessages : TheoryData<string, string, string, string, string>
{
    public RenamedMarkerMessages()
    {
        Add(
            "SPEC031",
            """{"schemaVersion":"0.3.0","markers":{"missing":"Untested"}}""",
            Frontmatter("") + Claims("B-001", "B-002") + Matrix("B-001", "`AlphaTest`"),
            "'Untested'",
            "'Missing'");
        Add(
            "SPEC031",
            """{"schemaVersion":"0.3.0","markers":{"withdrawn":"Retired"}}""",
            Frontmatter("") + Claims("B-001") + Matrix("B-001", "`AlphaTest`") + "| B-009 | It holds | `AlphaTest` | Covered |\n",
            "Retired",
            "Withdrawn");
        Add(
            "SPEC023",
            """{"schemaVersion":"0.4.0","markers":{"missing":"Untested"}}""",
            Frontmatter("") + Claims("B-001") + Matrix("B-001", "`AlphaTest`"),
            "'Untested'",
            "'Missing'");
        Add(
            "SPEC033",
            """{"schemaVersion":"0.4.0","markers":{"covered":"Proven"}}""",
            Frontmatter("") + Claims("B-001") + Matrix("B-009", "`AlphaTest`").Replace("| Covered |", "| Proven |", StringComparison.Ordinal),
            "'Proven'",
            "'Covered'");
        Add(
            "SPEC033",
            """{"schemaVersion":"0.4.0","markers":{"partial":"Halfway"}}""",
            Frontmatter("") + Claims("B-001") + Matrix("B-009", "`AlphaTest`").Replace("| Covered |", "| Halfway |", StringComparison.Ordinal),
            "'Halfway'",
            "'Partial'");
        Add(
            "SPEC060",
            """{"schemaVersion":"0.3.0","markers":{"missing":"TBD"}}""",
            Frontmatter("status: done\n") + Matrix("B-001", "TBD"),
            "'TBD'",
            "'Missing'");
        Add(
            "SPEC060",
            """{"schemaVersion":"0.3.0","markers":{"done":"delivered"}}""",
            Frontmatter("status: delivered\n") + Matrix("B-001", "Missing"),
            "'delivered'",
            "'done'");
        Add(
            "SPEC060",
            """{"markers":{"missing":"TBD"}}""",
            Frontmatter("spec_status: approved\n") + Matrix("B-001", "TBD"),
            "'TBD'",
            "'Missing'");
        Add(
            "SPEC060",
            """{"markers":{"approved":"agreed"}}""",
            Frontmatter("spec_status: agreed\n") + Matrix("B-001", "Missing"),
            "'agreed'",
            "'approved'");
        Add(
            "SPEC061",
            """{"schemaVersion":"0.3.0","markers":{"approved":"agreed"}}""",
            Frontmatter("spec_status: agreed\n")
                + "## 12. Sign-off\n\n| Section | Status | Reviewer | Note |\n| ------- | ------ | -------- | ---- |\n"
                + "| 1-5 | \U0001F7E1 | spec-reviewer | Draft |\n",
            "'agreed'",
            "'approved'");
    }

    private static string Frontmatter(string keys) => $"---\nepic: \"0001\"\nid: F1\n{keys}---\n\n";

    private static string Claims(params string[] claims) =>
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + string.Concat(claims.Select(static claim => $"| {claim} | It holds. | brd | Active |\n"))
            + "\n";

    private static string Matrix(string claim, string test) =>
        $"## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| -------- | -------- | ---- | ------ |\n"
            + $"| {claim} | It holds | {test} | Covered |\n";
}
