namespace Specht.Tests.Mcp;

/// <summary>The two places the server starts, each with whether it starts in another directory and names the repository's root (0132-F3 B-009).</summary>
public sealed class ServerStartingPlaces : TheoryData<bool>
{
    public ServerStartingPlaces()
    {
        Add(false);
        Add(true);
    }
}
