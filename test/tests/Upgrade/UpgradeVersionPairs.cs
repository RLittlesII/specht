namespace Specht.Tests.Upgrade;

/// <summary>
/// The (files version, pin) pairs of <c>0001-F7</c> B-005: an older version's files under a newer pin, and a newer's under an older pin.
/// </summary>
public sealed class UpgradeVersionPairs : TheoryData<string, string>
{
    public UpgradeVersionPairs()
    {
        Add("0.1.0", "0.2.0");
        Add("0.2.0", "0.1.0");
    }
}
