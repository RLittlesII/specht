namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A claim's § 3 Status, the tag its companion scenario carries, its § 9 Status, and whether the row is reported, under a
/// manifest that names the missing, waived, withdrawn and superseded markers "Absent", "Excused", "Retired" and "Replaced"
/// (B-011, A-3, C-5).
/// </summary>
public sealed class MissingScenarioRows : TheoryData<string, string, string, bool>
{
    public MissingScenarioRows()
    {
        const string untagged = "";
        Add("Active", untagged, "Covered", true);
        Add("Active", untagged, "Partial", true);
        Add("Active", untagged, "Retired", true);
        Add("Active", untagged, "Missing", true);
        Add("Active", untagged, "Waived", true);
        Add("Withdrawn", untagged, "Withdrawn", true);
        Add("Active", "@B-002", "Covered", true);
        Add("Active", "@B-001", "Covered", false);
        Add("Active", untagged, "Absent", false);
        Add("Active", untagged, "Excused", false);
        Add("Retired", untagged, "Retired", false);
        Add("Replaced", untagged, "Replaced", false);
    }
}
