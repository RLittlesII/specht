namespace Specht.Tests.Rules.Matrix;

/// <summary>Retirement markers, tag-case handling and matching coverage statuses.</summary>
public sealed class RetiredClaimCases : TheoryData<string, string, string?, bool, bool>
{
    public RetiredClaimCases()
    {
        Add("Retired", "Retired", "B-004", false, true);
        Add("Replaced", "Replaced", "B-004", false, true);
        Add("Retired", "Missing", null, false, true);
        Add("Replaced", "Retired", null, false, true);
        Add("Retired", "Missing", "B-004", false, true);
        Add("Retired", "Retired", null, false, false);
        Add("Replaced", "Replaced", null, false, false);
        Add("Active", "Missing", "B-004", false, false);
        Add("Withdrawn", "Missing", "B-004", false, false);
        Add("Retired", "Retired", "b-004", true, true);
        Add("Retired", "Retired", "b-004", false, false);
    }
}
