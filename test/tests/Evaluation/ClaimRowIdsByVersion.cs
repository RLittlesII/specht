namespace Specht.Tests.Evaluation;

/// <summary>
/// A pin, and the id a missing traceability row is reported under at that pin (<c>0001-F7</c> B-151, B-152): the
/// three-digit <c>SPEC031</c> under 0.1.0, 0.2.0 and 0.2.1, and the four-digit <c>SPEC4004</c> under 0.3.0.
/// </summary>
public sealed class ClaimRowIdsByVersion : TheoryData<string, string>
{
    /// <summary>Initializes one row per shipped version.</summary>
    public ClaimRowIdsByVersion()
    {
        Add("0.1.0", "SPEC031");
        Add("0.2.0", "SPEC031");
        Add("0.2.1", "SPEC031");
        Add("0.3.0", "SPEC4004");
    }
}
