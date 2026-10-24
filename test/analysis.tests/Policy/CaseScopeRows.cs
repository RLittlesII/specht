namespace Specht.Analysis.Tests.Policy;

public sealed class CaseScopeRows : TheoryData<string, string[], bool>
{
    public CaseScopeRows()
    {
        Add("@visual", ["visual"], true);
        Add("@visual", [], false);
        Add("not @visual", [], true);
        Add("not @visual", ["visual"], false);
        Add("not not @a", ["a"], true);
        Add("@a or @b and @c", ["b"], false);
        Add("@a or @b and @c", ["b", "c"], true);
        Add("(@a or @b) and @c", ["a"], false);
        Add("(@a or @b) and @c", ["a", "c"], true);
        Add("@a and not (@b or @c)", ["a", "c"], false);
        Add("@a and not (@b or @c)", ["a"], true);
        Add("@Visual", ["visual"], false);
        Add("@not", ["not"], true);
    }
}
