namespace Specht.TestEvidence.VsTest.Tests;

public sealed class UnresolvedMethodIdentityRows : TheoryData<string, string?, string?>
{
    public UnresolvedMethodIdentityRows()
    {
        Add("A.LocalCases.Run", "A.LocalCases", "Run");
        Add("A.LocalCases.Run", null, null);
        Add("A.LocalCases.Missing", null, null);
        Add("a.localcases.fact", null, null);
        Add("A.LocalCases.Fact", "A.LocalCases", null);
        Add("A.LocalCases.Fact", null, "Fact");
    }
}
