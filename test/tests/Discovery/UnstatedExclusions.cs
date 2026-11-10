namespace Specht.Tests.Discovery;

/// <summary>
/// Why a row walks as it does and the one exclusion its manifest declares: each an entry of OQ-19's (a) to (c), which no
/// claim states, so discovery inside a work tree walks and starts no git, keeping both modes one contract (<c>0001-F6</c>
/// C-3; § 7 "Design for item <c>0008</c>", case 4). What each entry excludes is OQ-19's, and no row states it.
/// </summary>
public sealed class UnstatedExclusions : TheoryData<string, string>
{
    public UnstatedExclusions()
    {
        Add("a / alone ends in a /, OQ-19's (a)", "/");
        Add("a trailing / is OQ-19's (b)", "/.spec/");
        Add("an empty segment is OQ-19's (c)", "/a//b");
    }
}
