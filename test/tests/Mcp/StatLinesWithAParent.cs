namespace Specht.Tests.Mcp;

/// <summary>Synthetic <c>/proc/&lt;pid&gt;/stat</c> lines, each with the parent process id it states.</summary>
public sealed class StatLinesWithAParent : TheoryData<string, int>
{
    public StatLinesWithAParent()
    {
        Add("4242 (sh) S 4100 4242 4100 34816 4242 4194304 101 0 0 0 0 0 0 0 20 0 1 0 98765 2408448 230\n", 4100);
        Add("4343 (a) (b c) S 4242 4343 4100 34816 4343 4194304 87 0 0 0 0 0 0 0 20 0 1 0 98766 2408448 198\n", 4242);
    }
}
