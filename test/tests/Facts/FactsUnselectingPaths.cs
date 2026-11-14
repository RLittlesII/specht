namespace Specht.Tests.Facts;

/// <summary>
/// Root-relative paths that select no specification of a tree holding <c>src/tool/.spec/README.md</c>,
/// <c>src/tool/Features/Facts/.spec/README.md</c> and <c>src/toolbox/.spec/README.md</c> (<c>0132-F2</c> B-020): a
/// directory holding none, a directory name that only begins a path segment, and a file beside a specification that is
/// not one.
/// </summary>
public sealed class FactsUnselectingPaths : TheoryData<string>
{
    public FactsUnselectingPaths()
    {
        Add("src/absent");
        Add("src/too");
        Add("src/tool/.spec/feature.feature");
    }
}
