namespace Specht.Tests.Mcp;

/// <summary>Two versions, the first the smaller; the numbers are compared as numbers, not as text.</summary>
public sealed class VersionOrders : TheoryData<string, string>
{
    public VersionOrders()
    {
        Add("0.3.0", "0.3.1");
        Add("0.3.1", "0.4.0");
        Add("0.9.99", "1.0.0");
        Add("0.1.9", "0.1.10");
    }
}
