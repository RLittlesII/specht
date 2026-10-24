namespace Specht.Analysis.Tests.Policy;

public sealed class InvalidCaseScopes : TheoryData<string>
{
    public InvalidCaseScopes()
    {
        Add("");
        Add("   ");
        Add("@a and");
        Add("not");
        Add("(@a");
        Add("@a)");
        Add("@a @b");
        Add("visual");
        Add("AND");
        Add("@");
    }
}
