namespace Specht.Tests.Mcp;

/// <summary>The two ways a root has no pin, each with whether the local tool manifest is removed rather than left listing no tool.</summary>
public sealed class UnpinnedManifests : TheoryData<bool>
{
    public UnpinnedManifests()
    {
        Add(true);
        Add(false);
    }
}
