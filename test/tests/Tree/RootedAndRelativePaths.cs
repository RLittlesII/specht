namespace Specht.Tests.Tree;

/// <summary>
/// A path a manifest writes, and whether it is rooted (<c>0001-F16</c> B-034, B-035; item <c>0395</c>): a rooted path, a
/// relative path that leaves the root, and a relative path inside it.
/// </summary>
public sealed class RootedAndRelativePaths : TheoryData<string, bool>
{
    /// <summary>Initializes one row per case.</summary>
    public RootedAndRelativePaths()
    {
        Add("/outside/README.md", true);
        Add("../outside/README.md", false);
        Add("README.md", false);
    }
}
