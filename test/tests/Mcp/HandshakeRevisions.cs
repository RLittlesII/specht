namespace Specht.Tests.Mcp;

/// <summary>The revisions that have an initialize handshake the host completes.</summary>
public sealed class HandshakeRevisions : TheoryData<string>
{
    public HandshakeRevisions()
    {
        Add("2025-06-18");
        Add("2025-11-25");
        Add("2024-11-05");
        Add("2025-03-26");
    }
}
