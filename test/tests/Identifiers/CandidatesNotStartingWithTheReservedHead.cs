namespace Specht.Tests.Identifiers;

/// <summary>
/// A one-letter Nano ID alphabet, a reserved head its candidate does not start with, or none, and the comparison
/// (<c>0001-F19</c> B-076, B-077; item <c>0437</c>): the other letter case under the case-sensitive claim tag setting, a longer
/// head the draw does not begin, as <c>ACTIVITY-SCENARIO-</c> beside <c>ACTIVITY-</c>, and a request with no reserved head.
/// </summary>
public sealed class CandidatesNotStartingWithTheReservedHead : TheoryData<string, string?, StringComparison>
{
    /// <summary>Initializes the letter-case row, the longer-head row and the no-head row.</summary>
    public CandidatesNotStartingWithTheReservedHead()
    {
        Add("s", "S", StringComparison.Ordinal);
        Add("S", "SCENARIO-", StringComparison.Ordinal);
        Add("S", null, StringComparison.Ordinal);
    }
}
