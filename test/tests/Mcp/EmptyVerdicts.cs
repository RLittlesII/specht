namespace Specht.Tests.Mcp;

/// <summary>Each exit code with which the check answers, for each of the two tools a verdict can come from, the pinned one and the global one.</summary>
public sealed class EmptyVerdicts : TheoryData<int, bool>
{
    public EmptyVerdicts()
    {
        Add(0, true);
        Add(1, true);
        Add(0, false);
        Add(1, false);
    }
}
