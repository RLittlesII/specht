namespace Specht.Tests.Manifest;

/// <summary>
/// Each shipped version below schema <c>0.3.1</c>, the first that knows <c>mint</c>, and each left unchanged by it
/// (<c>0001-F19</c> C-17; item <c>0413</c>, "0.1.0 to 0.3.0 unchanged").
/// </summary>
public sealed class PinsBeforeMint : TheoryData<string>
{
    /// <summary>Initializes the 0.1.0, 0.2.0, 0.2.1 and 0.3.0 rows.</summary>
    public PinsBeforeMint()
    {
        Add("0.1.0");
        Add("0.2.0");
        Add("0.2.1");
        Add("0.3.0");
    }
}
