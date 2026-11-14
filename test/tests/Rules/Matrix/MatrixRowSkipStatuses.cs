namespace Specht.Tests.Rules.Matrix;

/// <summary>
/// A traceability row's Status cell against the manifest's markers, with whether its tests are skipped (<c>0001-F13</c>
/// B-019): the waived marker the manifest gives, or the default when none is given, the cell, and the verdict. A row is
/// skipped when its trimmed Status is the waived, withdrawn or superseded marker, compared ordinally.
/// </summary>
public sealed class MatrixRowSkipStatuses : TheoryData<string?, string, bool>
{
    public MatrixRowSkipStatuses()
    {
        Add(null, "Waived", true);
        Add(null, "Withdrawn", true);
        Add(null, "Superseded", true);
        Add(null, " Superseded ", true);
        Add(null, "waived", false);
        Add(null, "Covered", false);
        Add(null, "Partial", false);
        Add(null, "Missing", false);
        Add(null, string.Empty, false);
        Add("Excused", "Excused", true);
        Add("Excused", "Waived", false);
    }
}
