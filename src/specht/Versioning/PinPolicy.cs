namespace Specht.Versioning;

/// <summary>
/// How a manifest's pin selects the version the check uses (<c>0001-F7</c> B-053, B-054, B-055, B-147; decisions 0020 and
/// 0021).
/// </summary>
/// <implements epic="0001" feature="F7" claim="B-053" />
/// <implements epic="0001" feature="F7" claim="B-054" />
/// <implements epic="0001" feature="F7" claim="B-147" />
public enum PinPolicy
{
    /// <summary>The pinned version itself, <c>exact</c>, and the policy of a manifest with no <c>pinPolicy</c> (B-055, B-147).</summary>
    Exact,

    /// <summary>The highest shipped version with the pin's major and minor, <c>latest-patch</c> (B-053).</summary>
    LatestPatch,

    /// <summary>The highest shipped version with the pin's major, <c>latest-minor</c>, from <c>1.0.0</c> (B-054, B-150).</summary>
    LatestMinor,
}
