namespace Specht.Tests.Versioning;

/// <summary>
/// Each shipped version below 1.0.0 beside the shipped version higher than it in its patch alone (<c>0001-F7</c> B-154):
/// 0.2.0 and 0.2.1, and 0.3.0 and 0.3.1 (item 0388).
/// </summary>
public sealed class ShippedPatchNeighbours : TheoryData<string, string>
{
    /// <summary>Initializes the 0.2.0 and 0.3.0 rows.</summary>
    public ShippedPatchNeighbours()
    {
        Add("0.2.0", "0.2.1");
        Add("0.3.0", "0.3.1");
    }
}
