namespace Specht.Tests.Manifest;

/// <summary>
/// A <c>pinPolicy</c> value as the manifest writes it in JSON, other than <c>exact</c>, <c>latest-patch</c> and
/// <c>latest-minor</c>, and the text a rejection names it by (<c>0001-F7</c> B-056).
/// </summary>
public sealed class UndefinedPinPolicies : TheoryData<string, string>
{
    /// <summary>Initializes the scenario's two values, a defined value in other letter case, and three values that are not strings, null among them.</summary>
    public UndefinedPinPolicies()
    {
        Add("\"latest-major\"", "latest-major");
        Add("\"latest\"", "latest");
        Add("\"Latest-Patch\"", "Latest-Patch");
        Add("42", "42");
        Add("true", "true");
        Add("null", "null");
    }
}
