namespace Specht.Tests.Mint;

/// <summary>
/// The number of attempts an <c>adr</c> entry names, or none, and how many of its first candidates the tree holds: as many
/// as the guard tries, 8 when the entry names none (<c>0001-F19</c> B-036) and the entry's own number when it names one
/// (B-037). The candidate after the held ones is free, so a guard that tries one more prints it.
/// </summary>
public sealed class GuardsHoldingEveryAttempt : TheoryData<int?, int>
{
    /// <summary>Initializes the row with no attempts named and the row naming 3.</summary>
    public GuardsHoldingEveryAttempt()
    {
        Add(null, 8);
        Add(3, 3);
    }
}
