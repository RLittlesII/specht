namespace Specht.Tests.IdGeneration;

/// <summary>
/// Nano ID sizes a type may configure (<c>0001-F19</c> B-005): one character, the per-Feature and repository-wide
/// sizes of ADR-0033, the scenario's size of 10, and the url-safe default's 21.
/// </summary>
public sealed class NanoIdSizes : TheoryData<int>
{
    /// <summary>Initializes the 1, 6, 8, 10 and 21 rows.</summary>
    public NanoIdSizes()
    {
        Add(1);
        Add(6);
        Add(8);
        Add(10);
        Add(21);
    }
}
