using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>
/// A pin, the <c>pinPolicy</c> a manifest writes beside it, null for none, and the policy the parsed structure carries
/// (<c>0001-F7</c> B-053, B-054, B-055, B-147).
/// </summary>
public sealed class PinPolicyValues : TheoryData<string, string?, PinPolicy>
{
    /// <summary>Initializes the row with no key and one row for each value the engine defines.</summary>
    public PinPolicyValues()
    {
        Add("0.3.0", null, PinPolicy.Exact);
        Add("0.3.0", "exact", PinPolicy.Exact);
        Add("0.3.0", "latest-patch", PinPolicy.LatestPatch);
        Add("1.0.0", "latest-minor", PinPolicy.LatestMinor);
    }
}
