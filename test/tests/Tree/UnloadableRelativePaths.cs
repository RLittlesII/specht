namespace Specht.Tests.Tree;

/// <summary>
/// Why a path a manifest writes loads nothing, and the path, under a root <c>tree</c> holding <c>README.md</c> and a
/// <c>locked.md</c> that cannot be read, beside a file <c>outside/README.md</c> outside it (<c>0001-F16</c> B-033 to
/// B-035; item <c>0395</c>): a rooted path to the file outside, a relative path leading to it, a path naming no file, the
/// root itself, and the file that cannot be read.
/// </summary>
public sealed class UnloadableRelativePaths : TheoryData<string, string>
{
    /// <summary>Initializes one row per case.</summary>
    public UnloadableRelativePaths()
    {
        Add("a rooted path is refused before it is read", "/outside/README.md");
        Add("a path that leaves the root is refused", "../outside/README.md");
        Add("a path that names no file loads nothing", "missing.md");
        Add("a path that names the root loads nothing", ".");
        Add("a file that cannot be read loads nothing", "locked.md");
    }
}
