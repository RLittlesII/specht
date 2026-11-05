namespace Specht.Tests.Mcp;

/// <summary>The two tools the MCP can run, each with whether it is the pinned one; the other is the global one a root with no pin falls back to.</summary>
public sealed class PinnedOrGlobalTools : TheoryData<bool>
{
    public PinnedOrGlobalTools()
    {
        Add(true);
        Add(false);
    }
}
