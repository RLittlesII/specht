namespace Specht.Tests.Facts;

/// <summary>
/// A role the manifest's <c>roles</c> renames, its default and its new section title, the member it fills, and the
/// headers and one row of the table under it (<c>0132-F2</c> B-016): the claims section as <c>3. Requirements</c> and
/// the sign-off section as <c>12. Approval</c>, the scenario's examples.
/// </summary>
public sealed class FactsRenamedRoles : TheoryData<string, string, string, string, string[], string[]>
{
    public FactsRenamedRoles()
    {
        Add(
            "claims",
            "3. Acceptance Criteria",
            "3. Requirements",
            "claims",
            ["ID", "Claim", "Source", "Status"],
            ["B-001", "It holds.", "the owner", "Active"]);
        Add(
            "signOff",
            "12. Sign-off",
            "12. Approval",
            "signOff",
            ["Section", "Status", "Reviewer", "Note"],
            ["1-5", "approved", "spec-reviewer", "Round 1."]);
    }
}
