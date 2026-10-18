namespace Specht.Analysis.Tests.Bindings;

public sealed class FrontmatterIdentityCases : TheoryData<string, string?>
{
    public FrontmatterIdentityCases()
    {
        Add("0400", "0400");
        Add("'true'", "true");
        Add("2026-10-17", "2026-10-17");
        Add("true", null);
        Add("42", null);
        Add("null", null);
    }
}
