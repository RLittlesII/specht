namespace Specht.TestEvidence.VsTest.Tests;

public sealed class ResolvedMethodIdentityRows : TheoryData<string, string?, string?, string>
{
    public ResolvedMethodIdentityRows()
    {
        Add("A.LocalCases.Theory", "A.LocalCases", "Theory", "M:A.LocalCases.Theory(System.Int32)");
        Add("Elsewhere.Named", "A.LocalCases", "Theory", "M:A.LocalCases.Theory(System.Int32)");
        Add("A.LocalCases.Fact", null, null, "M:A.LocalCases.Fact");
    }
}
