namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A § 9 row of claim B-001, under declared test files and the markers <see cref="StatusRows"/> names: the § 3 row, the
/// companion, the level <c>rules</c> gives <c>SPEC4008</c> or null for none, the Test cell, the Status, and the ids
/// <c>SPEC4008</c>, <c>SPEC4009</c> and <c>SPEC4010</c> report the row under, together (B-012, B-013, B-028, B-029, B-006,
/// B-011, B-015, B-026). The declared file holds <c>AlphaTest</c> and
/// <c>BetaTest</c>.
/// </summary>
public sealed class StatusBesideOtherRules : TheoryData<string, string, string?, string, string, string[]>
{
    public StatusBesideOtherRules()
    {
        const string live = "B-001 | Behavior | Decision | Active";
        const string uncarried = "Feature: Sample\n\n  Scenario: First case\n    Given a thing\n";
        const string carried = "Feature: Sample\n\n  @B-001\n  Scenario: First case\n    Given a thing\n";
        Add(live, uncarried, null, "`AlphaTest`", "Proven", ["SPEC4008"]);
        Add(live, uncarried, null, "Absent", "Halfway", ["SPEC4008"]);
        Add(live, uncarried, null, "`AlphaTest`", "Done", ["SPEC4008"]);
        Add(live, uncarried, null, "`AlphaTest`, `BetaTest`", "Absent", ["SPEC4010"]);
        Add(live, carried, null, "AlphaTest", "Proven", ["SPEC4009", "SPEC4010"]);
        Add("B-001 | Behavior | Decision | Retired", uncarried, null, "`AlphaTest`, `BetaTest`", "Proven", ["SPEC4010"]);
        Add("B-001 | Behavior | Decision | Replaced", uncarried, null, "`AlphaTest`, `BetaTest`", "Proven", ["SPEC4010"]);
        Add("B-002 | Behavior | Decision | Active", uncarried, null, "`AlphaTest`, `BetaTest`", "Proven", ["SPEC4010"]);
        Add(live, $"{uncarried}\n  @B-001\n  Rule: A rule\n", null, "`AlphaTest`, `BetaTest`", "Proven", []);
        Add(live, uncarried, "warning", "`AlphaTest`, `BetaTest`", "Proven", ["SPEC4008"]);
        Add(live, uncarried, "off", "`AlphaTest`, `BetaTest`", "Proven", []);
        Add(live, "@B-001\nFeature: Sample\n", null, "`AlphaTest`, `BetaTest`", "Proven", []);
    }
}
