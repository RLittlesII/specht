namespace Specht.TestEvidence.VsTest.Tests;

public sealed class OwnershipRows : TheoryData<string?, string[], bool>
{
    public OwnershipRows()
    {
        Add(null, ["visual"], true);
        Add(null, [], true);
        Add("not @visual", [], true);
        Add("not @visual", ["visual"], false);
        Add("@visual", ["visual"], true);
        Add("@visual", [], false);
    }
}
