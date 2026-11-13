namespace Specht.Tests.Manifest;

/// <summary>Each shipped version below schema <c>0.4.0</c>, the first that knows <c>ids</c> (<c>0001-F5</c> B-082, A-8).</summary>
public sealed class PinsBeforeIds : TheoryData<string>
{
    /// <summary>Initializes the 0.1.0, 0.2.0, 0.2.1, 0.3.0 and 0.3.1 rows.</summary>
    public PinsBeforeIds()
    {
        Add("0.1.0");
        Add("0.2.0");
        Add("0.2.1");
        Add("0.3.0");
        Add("0.3.1");
    }
}
