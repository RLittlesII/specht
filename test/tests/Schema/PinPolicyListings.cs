namespace Specht.Tests.Schema;

/// <summary>
/// The versions a set holds beside a manifest pinning <c>0.3.0</c> with <c>latest-patch</c>, and the version the check
/// uses (<c>0001-F7</c> B-148, B-149): a later patch held is in use, and with none held the pin is.
/// </summary>
public sealed class PinPolicyListings : TheoryData<string[], string>
{
    /// <summary>Initializes the B-148 and B-149 rows.</summary>
    public PinPolicyListings()
    {
        Add(["0.3.0", "0.3.1"], "0.3.1");
        Add(["0.3.0", "0.4.0"], "0.3.0");
    }
}
