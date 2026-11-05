namespace Specht.Tests.Mcp;

/// <summary>The two shipped schema versions a repository is pinned to in turn (0132-F4 B-013).</summary>
public sealed class ShippedSchemaVersions : TheoryData<string>
{
    public ShippedSchemaVersions()
    {
        Add("0.1.0");
        Add("0.2.1");
    }
}
