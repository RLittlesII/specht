namespace Specht.Tests.Identifiers;

/// <summary>
/// A one-letter Nano ID alphabet, a reserved head its candidate starts with, and the comparison it starts with it under
/// (<c>0001-F19</c> B-076, B-077; item <c>0437</c>): the letter case as drawn under the case-sensitive claim tag setting, and the
/// other letter case where the setting ignores case.
/// </summary>
public sealed class CandidatesStartingWithTheReservedHead : TheoryData<string, string, StringComparison>
{
    /// <summary>Initializes the case-sensitive row and the ignore-case row.</summary>
    public CandidatesStartingWithTheReservedHead()
    {
        Add("S", "S", StringComparison.Ordinal);
        Add("s", "S", StringComparison.OrdinalIgnoreCase);
    }
}
