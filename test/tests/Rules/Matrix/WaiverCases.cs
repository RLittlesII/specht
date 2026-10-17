namespace Specht.Tests.Rules.Matrix;

/// <summary>Configured waiver markers, signed reviewers and invariant date boundaries.</summary>
public sealed class WaiverCases : TheoryData<string, string, string, string, bool>
{
    public WaiverCases()
    {
        Add("Excused", "reviewer, 2026-10-14", "Accepted", "reviewer", false);
        Add("Excused", "reviewer", "Accepted", "reviewer", true);
        Add("Excused", "reviewer, 2026-02-30", "Accepted", "reviewer", true);
        Add("Excused", "reviewer, 2026-1-4", "Accepted", "reviewer", true);
        Add("Excused", "reviewer, 2026-10-14", "DraftState", "reviewer", true);
        Add("Excused", "reviewer, 2026-10-14", "BlockedState", "reviewer", true);
        Add("Excused", "other, 2026-10-14", "Accepted", "reviewer", true);
        Add("Excused", "", "Accepted", "reviewer", true);
        Add("Excused", "reviewer, 2026-10-14", "Accepted", "", true);
        Add("Covered", "", "DraftState", "reviewer", false);
        Add("Waived", "", "DraftState", "reviewer", false);
    }
}
