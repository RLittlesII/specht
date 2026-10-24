namespace Specht.TestEvidence.VsTest.Tests;

public sealed class NoCurrentDiscoveryRows : TheoryData<bool>
{
    public NoCurrentDiscoveryRows()
    {
        Add(false);
        Add(true);
    }
}
