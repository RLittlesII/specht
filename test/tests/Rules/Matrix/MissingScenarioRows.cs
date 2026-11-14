namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A claim's § 3 Status, its companion, the Status of each of its § 9 rows, and the line of each row reported, under a
/// manifest that names the missing, waived, withdrawn and superseded markers "Absent", "Excused", "Retired" and "Replaced"
/// (B-011, B-026, B-027, A-3, C-5). The claim's rows start at line 12.
/// </summary>
public sealed class MissingScenarioRows : TheoryData<string, string, string[], int[]>
{
    public MissingScenarioRows()
    {
        const string untagged = "Feature: Sample\n\n  Scenario: A scenario\n    Given a thing\n";
        const string tagged = "Feature: Sample\n\n  @B-001\n  Scenario: A scenario\n    Given a thing\n";
        const string otherClaim = "Feature: Sample\n\n  @B-002\n  Scenario: A scenario\n    Given a thing\n";
        const string emptyRule = "Feature: Sample\n\n  Scenario: A scenario\n    Given a thing\n\n  @B-001\n  Rule: A rule\n";
        const string emptyFeature = "@B-001\nFeature: Sample\n";
        Add("Active", untagged, ["Covered"], [12]);
        Add("Active", untagged, ["Partial"], [12]);
        Add("Active", untagged, ["Retired"], [12]);
        Add("Active", untagged, ["Missing"], [12]);
        Add("Active", untagged, ["Waived"], [12]);
        Add("Withdrawn", untagged, ["Withdrawn"], [12]);
        Add("Active", otherClaim, ["Covered"], [12]);
        Add("Active", tagged, ["Covered"], []);
        Add("Active", untagged, ["Absent"], []);
        Add("Active", untagged, ["Excused"], []);
        Add("Retired", untagged, ["Retired"], []);
        Add("Replaced", untagged, ["Replaced"], []);
        Add("Active", untagged, ["Covered", "Partial"], [12, 13]);
        Add("Active", untagged, ["Absent", "Covered"], [13]);
        Add("Active", untagged, [], []);
        Add("Active", emptyRule, ["Covered"], []);
        Add("Active", emptyFeature, ["Covered"], []);
    }
}
