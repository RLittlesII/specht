namespace Specht.Tests.Facts;

/// <summary>
/// A shipped <c>sections</c> entry and the entries the manifest lists in its place, the manifest's <c>roles</c>, the
/// member read, the section titles the specification carries in document order, and the lines of the rows that member
/// gives: the first <c>sections</c> entry whose leading number is 4, 5, 10 or 11, the run of digits that opens it, and
/// that no <c>roles</c> entry names, or none (<c>0132-F2</c> B-016, C-7, decision 0003, OQ-8's default). Each title in
/// the specification carries one row, at line 24 for the first title and ten lines on for each after it.
/// </summary>
public sealed class FactsNumberedSections : TheoryData<string, string[], string, string, string[], int[]>
{
    public FactsNumberedSections()
    {
        Add("4. Constraints", ["4. Limits"], "{}", "constraints", ["4. Constraints", "4. Limits"], [34]);
        Add("5. Out of Scope", ["5. Exclusions"], "{}", "outOfScope", ["5. Out of Scope", "5. Exclusions"], [34]);
        Add("10. Lessons / Spec Deltas", ["10. Changes"], "{}", "deltas", ["10. Lessons / Spec Deltas", "10. Changes"], [34]);
        Add("11. Open Questions", ["11. Questions"], "{}", "openQuestions", ["11. Open Questions", "11. Questions"], [34]);
        Add("4. Constraints", ["4 Limits"], "{}", "constraints", ["4. Constraints", "4 Limits"], [34]);
        Add("4. Constraints", ["Limits"], "{}", "constraints", ["4. Constraints", "Limits"], []);
        Add("4. Constraints", ["3. Limits"], "{}", "constraints", ["4. Constraints", "3. Limits"], []);
        Add("4. Constraints", ["40. Limits"], "{}", "constraints", ["4. Constraints", "40. Limits"], []);
        Add("5. Out of Scope", ["Exclusions"], "{}", "outOfScope", ["5. Out of Scope", "Exclusions"], []);
        Add("10. Lessons / Spec Deltas", ["1. Changes"], "{}", "deltas", ["10. Lessons / Spec Deltas", "1. Changes"], []);
        Add("11. Open Questions", ["110. Questions"], "{}", "openQuestions", ["11. Open Questions", "110. Questions"], []);
        Add("4. Constraints", ["4. Limits", "4. Constraints"], "{}", "constraints", ["4. Constraints", "4. Limits"], [34]);
        Add(
            "3. Acceptance Criteria",
            ["4. Requirements"],
            """{"claims":"4. Requirements"}""",
            "constraints",
            ["4. Requirements", "4. Constraints"],
            [34]);
        Add(
            "11. Open Questions",
            ["11. Approval", "11. Open Questions"],
            """{"signOff":"11. Approval"}""",
            "openQuestions",
            ["11. Approval", "11. Open Questions"],
            [34]);
        Add("4. Constraints", ["4. Requirements"], """{"claims":"4. Requirements"}""", "constraints", ["4. Requirements", "4. Constraints"], []);
    }
}
