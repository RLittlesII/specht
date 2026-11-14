namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A § 9 row of a live claim, under declared test files and the markers <see cref="StatusRows"/> names: whether a companion
/// scenario carries the claim, the Test cell, the Status, and the ids <c>SPEC4008</c>, <c>SPEC4009</c> and <c>SPEC4010</c>
/// report the row under, together (B-012, B-013, B-006, B-011, B-015). The declared file holds <c>AlphaTest</c> and
/// <c>BetaTest</c>.
/// </summary>
public sealed class StatusBesideOtherRules : TheoryData<bool, string, string, string[]>
{
    public StatusBesideOtherRules()
    {
        Add(false, "`AlphaTest`", "Proven", ["SPEC4008"]);
        Add(false, "Absent", "Halfway", ["SPEC4008"]);
        Add(false, "`AlphaTest`", "Done", ["SPEC4008"]);
        Add(false, "`AlphaTest`, `BetaTest`", "Absent", ["SPEC4010"]);
        Add(true, "AlphaTest", "Proven", ["SPEC4009", "SPEC4010"]);
    }
}
