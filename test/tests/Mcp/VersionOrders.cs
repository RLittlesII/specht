namespace Specht.Tests.Mcp;

/// <summary>Two versions, the first the smaller; the numbers are compared as numbers, not as text.</summary>
public sealed class VersionOrders : TheoryData<string, string>
{
    public VersionOrders()
    {
        Add("0.1.89", "0.1.90");
        Add("0.1.90", "0.2.0");
        Add("0.9.99", "1.0.0");
        Add("0.1.9", "0.1.10");
    }
}
