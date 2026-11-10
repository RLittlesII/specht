namespace Specht.Tests.Mcp;

/// <summary>Synthetic <c>/proc/&lt;pid&gt;/stat</c> text that is empty or ends before the parent process id.</summary>
public sealed class StatLinesWithNoParent : TheoryData<string>
{
    public StatLinesWithNoParent()
    {
        Add(string.Empty);
        Add("4242 (sh");
        Add("4242 (sh)");
        Add("4242 (sh) S");
    }
}
