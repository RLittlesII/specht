namespace Specht.Tests.Facts;

/// <summary>
/// A manifest, a sign-off table's headers and one row under them, and the section, status, reviewer and note the facts
/// give for it (<c>0132-F2</c> B-010, B-016, B-017): under schema <c>0.1.0</c>, whose <c>tables</c> names no sign-off
/// headers, the engine's; under a <c>tables.signOff</c> that renames every header, those names, at <c>0.1.0</c> and
/// <c>0.2.0</c>; columns in another order, each read by its header; a header the table lacks giving <see langword="null"/>;
/// an empty cell giving <c>""</c>; and a draft or blocked status given as written.
/// </summary>
public sealed class FactsSignOffTables : TheoryData<string, string[], string[], string?, string?, string?, string?>
{
    public FactsSignOffTables()
    {
        Add(
            """{"schemaVersion":"0.1.0"}""",
            ["Section", "Status", "Reviewer", "Note"],
            ["1-5", "\U0001F7E1", "spec-reviewer", "Draft"],
            "1-5",
            "\U0001F7E1",
            "spec-reviewer",
            "Draft");
        Add(
            """{"schemaVersion":"0.2.0","tables":{"matrix":["Claim ID","Scenario","Test","Status"],"signOff":["Part","State","By","Comment"]}}""",
            ["Part", "State", "By", "Comment"],
            ["6-9", "approved", "spec-reviewer", "Round 2 - approved."],
            "6-9",
            "approved",
            "spec-reviewer",
            "Round 2 - approved.");
        Add(
            """{"schemaVersion":"0.1.0","tables":{"matrix":["Claim ID","Scenario","Test","Status"],"signOff":["Part","State","By","Comment"]}}""",
            ["Part", "State", "By", "Comment"],
            ["6-9", "approved", "spec-reviewer", "Round 2 - approved."],
            "6-9",
            "approved",
            "spec-reviewer",
            "Round 2 - approved.");
        Add(
            """{"schemaVersion":"0.2.0"}""",
            ["Reviewer", "Section", "Note", "Status"],
            ["spec-reviewer", "1-5", "Round 1 - blocked.", "\U0001F534"],
            "1-5",
            "\U0001F534",
            "spec-reviewer",
            "Round 1 - blocked.");
        Add(
            """{"schemaVersion":"0.2.0"}""",
            ["Section", "Status", "Reviewer"],
            ["1-5", "approved", "spec-reviewer"],
            "1-5",
            "approved",
            "spec-reviewer",
            null);
        Add(
            """{"schemaVersion":"0.2.0"}""",
            ["Section", "Status", "Reviewer", "Note"],
            ["1-5", "approved", "spec-reviewer", ""],
            "1-5",
            "approved",
            "spec-reviewer",
            "");
    }
}
