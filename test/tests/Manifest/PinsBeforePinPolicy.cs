namespace Specht.Tests.Manifest;

/// <summary>Each shipped version below schema <c>0.3.0</c>, the first that knows <c>pinPolicy</c> (<c>0001-F7</c> B-153, decision 0021).</summary>
public sealed class PinsBeforePinPolicy : TheoryData<string>
{
    /// <summary>Initializes the 0.1.0, 0.2.0 and 0.2.1 rows.</summary>
    public PinsBeforePinPolicy()
    {
        Add("0.1.0");
        Add("0.2.0");
        Add("0.2.1");
    }
}
