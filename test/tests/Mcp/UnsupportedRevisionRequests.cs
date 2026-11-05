namespace Specht.Tests.Mcp;

/// <summary>Requests that name a protocol revision the MCP does not support: one no revision carries, and one that has an initialize handshake.</summary>
public sealed class UnsupportedRevisionRequests : TheoryData<string, string>
{
    public UnsupportedRevisionRequests()
    {
        Add("server/discover", "1900-01-01");
        Add("tools/list", "1900-01-01");
        Add("server/discover", "2025-11-25");
        Add("tools/list", "2025-11-25");
    }
}
