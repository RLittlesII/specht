namespace Specht.Tests.Discovery;

/// <summary>
/// Why a row asks as it does, and a directory at the root whose name begins with a character a glob pathspec reads as
/// special, so the glob <c>&lt;directory&gt;/**/*.md</c> is written with each of <c>Bracketed</c>'s spellings of its head
/// and run against a real git (<c>0001-F6</c> B-007, B-010; item 0009).
/// </summary>
public sealed class GlobHeads : TheoryData<string, string>
{
    public GlobHeads()
    {
        Add("a [ head is written [[], a bracket holding the bracket", "[x]");
        Add("a ? head is written [?]", "?x");
        Add("a ! head is written []!], so it does not negate", "!x");
        Add("a ^ head is written []^], so it does not negate", "^x");
        Add("a ] head is written []]", "]x");
        Add(@"a \ head is written \\, escaped as before", @"\x");
    }
}
