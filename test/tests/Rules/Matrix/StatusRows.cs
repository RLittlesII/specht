namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A § 9 row of a live claim a companion scenario carries, under a manifest that names the covered, partial, missing, waived,
/// withdrawn and superseded markers "Proven", "Halfway", "Absent", "Excused", "Retired" and "Replaced" (C-5): whether
/// <c>tests.files</c> is declared, whether <c>matrix.scenarioTitles</c> is on, the Scenario cell, the Test cell, the Status,
/// and the text the one finding's message holds, or null when the row is not reported (B-013 to B-017, B-030, A-1, A-2). The
/// companion's <c>First case</c> carries the claim and its <c>Other case</c> does not; the declared file holds
/// <c>AlphaTest</c> and <c>BetaTest</c>.
/// </summary>
public sealed class StatusRows : TheoryData<bool, bool, string, string, string, string[]?>
{
    public StatusRows()
    {
        string[] reported = [];
        string[] stale = ["stale"];
        string[] markers = ["Proven", "Halfway", "Absent", "Excused", "Retired", "Replaced"];
        Add(true, true, "First case", "`AlphaTest`, `BetaTest`", "Proven", null);
        Add(true, true, "A title none has", "`AlphaTest`, `BetaTest`", "Proven", reported);
        Add(true, true, "Other case", "`AlphaTest`, `BetaTest`", "Proven", reported);
        Add(true, true, "First case; A title none has", "`AlphaTest`, `BetaTest`", "Proven", reported);
        Add(true, true, "First case", "`AlphaTest`, `GammaTest`", "Proven", reported);
        Add(true, true, "First case", "Absent", "Proven", reported);
        Add(true, false, "A title none has", "`AlphaTest`, `BetaTest`", "Proven", null);
        Add(true, true, "", "`AlphaTest`, `BetaTest`", "Proven", reported);
        Add(true, true, " ; ; ", "`AlphaTest`, `BetaTest`", "Proven", reported);
        Add(true, false, "", "`AlphaTest`, `BetaTest`", "Proven", null);
        Add(true, false, "First case", "`GammaTest`", "Proven", reported);
        Add(true, true, "First case", "`GammaTest`, `DeltaTest`", "Halfway", null);
        Add(true, true, "A title none has", "`AlphaTest`, `GammaTest`", "Halfway", null);
        Add(true, true, "A title none has", "`GammaTest`, `DeltaTest`", "Halfway", reported);
        Add(true, true, "A title none has", "Absent", "Halfway", reported);
        Add(true, false, "First case", "`AlphaTest`, `BetaTest`", "Absent", stale);
        Add(true, false, "First case", "`AlphaTest`, `GammaTest`", "Absent", null);
        Add(true, false, "First case", "Absent", "Absent", null);
        Add(false, false, "First case", "`GammaTest`", "Proven", null);
        Add(false, false, "First case", "`AlphaTest`, `BetaTest`", "Absent", null);
        Add(false, false, "First case", "Absent", "Proven", reported);
        Add(false, false, "First case", "AlphaTest", "Proven", reported);
        Add(false, true, "A title none has", "`AlphaTest`", "Proven", reported);
        Add(false, true, "A title none has", "`AlphaTest`", "Halfway", reported);
        Add(false, true, "First case", "`GammaTest`", "Halfway", null);
        Add(true, false, "First case", "`AlphaTest`", "Done", markers);
        Add(true, false, "First case", "`AlphaTest`, `BetaTest`", "Missing", markers);
        Add(true, true, "A title none has", "`GammaTest`", "Excused", null);
        Add(true, true, "A title none has", "`GammaTest`", "Retired", null);
        Add(true, true, "A title none has", "`GammaTest`", "Replaced", null);
    }
}
