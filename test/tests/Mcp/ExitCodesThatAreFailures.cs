namespace Specht.Tests.Mcp;

/// <summary>The exit codes with which the check reports a failure: no root or manifest, an invalid manifest, a thing not found.</summary>
public sealed class ExitCodesThatAreFailures : TheoryData<int>
{
    public ExitCodesThatAreFailures()
    {
        Add(2);
        Add(3);
        Add(4);
    }
}
