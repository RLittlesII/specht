namespace Specht.Tests.Schema;

/// <summary>
/// The versions a set holds beside a manifest pinning <c>0.3.0</c> with <c>latest-patch</c>, and the lines the listing
/// prints (<c>0001-F7</c> B-084, B-148, B-149): the pin marked <c> (pinned)</c>, and the version in use marked
/// <c> (in use)</c> only when it is not the pin.
/// </summary>
public sealed class PinPolicyMarks : TheoryData<string[], string[]>
{
    /// <summary>Initializes the B-148 and B-149 rows.</summary>
    public PinPolicyMarks()
    {
        Add(["0.3.0", "0.3.1"], ["0.3.0 (pinned)", "0.3.1 (in use)"]);
        Add(["0.3.0", "0.4.0"], ["0.3.0 (pinned)", "0.4.0"]);
    }
}
