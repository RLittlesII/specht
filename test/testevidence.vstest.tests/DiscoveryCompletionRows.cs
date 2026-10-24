namespace Specht.TestEvidence.VsTest.Tests;

public sealed class DiscoveryCompletionRows : TheoryData<bool, bool, bool, bool, string>
{
    public DiscoveryCompletionRows()
    {
        Add(true, false, false, false, "unavailable");
        Add(false, true, false, false, "deferred");
        Add(false, false, true, false, "deferred");
        Add(false, false, false, true, "deferred");
    }
}
