using Specht.Versioning;

namespace Specht.Tests.Versioning;

/// <summary>
/// A held set, a pin it holds, a pin policy and the version that policy has the check use (<c>0001-F7</c> B-053, B-054,
/// B-147): <c>latest-patch</c> takes the highest held version with the pin's major and minor, <c>latest-minor</c> the
/// highest with the pin's major, and <c>exact</c> the pin, each set given out of order.
/// </summary>
public sealed class PinPolicySelections : TheoryData<string[], string, PinPolicy, string>
{
    /// <summary>Initializes the rows of each policy.</summary>
    public PinPolicySelections()
    {
        Add(["0.3.0", "1.0.0", "0.3.2", "0.4.0", "0.3.1"], "0.3.0", PinPolicy.LatestPatch, "0.3.2");
        Add(["0.4.0", "0.3.0"], "0.3.0", PinPolicy.LatestPatch, "0.3.0");
        Add(["1.0.0", "2.0.0", "1.2.1", "1.1.0", "1.2.0"], "1.0.0", PinPolicy.LatestMinor, "1.2.1");
        Add(["1.0.0", "2.0.0"], "1.0.0", PinPolicy.LatestMinor, "1.0.0");
        Add(["0.3.0", "0.3.1", "0.4.0", "1.0.0"], "0.3.0", PinPolicy.Exact, "0.3.0");
    }
}
