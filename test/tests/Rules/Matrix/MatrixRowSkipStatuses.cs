namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A traceability row's Status cell against the manifest's markers, with whether its tests are skipped (<c>0001-F13</c>
/// B-019): the pinned schema version, the waived marker the manifest gives, or the default when none is given, the cell,
/// and the verdict. A row is skipped when its trimmed Status is the waived, withdrawn or superseded marker, compared
/// ordinally; under a <c>0.1.0</c> pin the manifest has none of the three, so nothing is skipped.
/// </summary>
public sealed class MatrixRowSkipStatuses : TheoryData<string, string?, string, bool>
{
    public MatrixRowSkipStatuses()
    {
        Add("0.2.0", null, "Waived", true);
        Add("0.2.0", null, "Withdrawn", true);
        Add("0.2.0", null, "Superseded", true);
        Add("0.2.0", null, " Superseded ", true);
        Add("0.2.0", null, "waived", false);
        Add("0.2.0", null, "Covered", false);
        Add("0.2.0", null, "Partial", false);
        Add("0.2.0", null, "Missing", false);
        Add("0.2.0", null, string.Empty, false);
        Add("0.2.0", "Excused", "Excused", true);
        Add("0.2.0", "Excused", "Waived", false);
        Add("0.1.0", null, "Waived", false);
        Add("0.1.0", null, "Withdrawn", false);
        Add("0.1.0", null, "Superseded", false);
    }
}
