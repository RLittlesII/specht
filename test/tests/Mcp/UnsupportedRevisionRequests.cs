namespace Specht.Tests.Mcp;

/// <summary>Requests that name a protocol revision the MCP does not support: one no revision carries.</summary>
public sealed class UnsupportedRevisionRequests : TheoryData<string, string>
{
    public UnsupportedRevisionRequests()
    {
        Add("server/discover", "1900-01-01");
        Add("tools/list", "1900-01-01");
    }
}
