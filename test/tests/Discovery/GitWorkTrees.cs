namespace Specht.Tests.Discovery;

/// <summary>
/// Why a row lists as it does, the root, the <c>.git</c> directory or file a row puts at it or above it, whether git can be
/// started, how many times discovery starts it, and the specifications it finds, git's canned listing naming
/// <c>src/a</c>'s alone and the tree holding <c>src/a</c>'s and <c>src/b</c>'s (<c>0001-F6</c> B-005, B-006).
/// </summary>
public sealed class GitWorkTrees : TheoryData<string, string, string?, string?, bool, int, string[]>
{
    public GitWorkTrees()
    {
        Add("no .git at the root or above it is outside a work tree, so the tree is walked", "repo", null, null, true, 0, [A, B]);
        Add("a .git directory at the root is a work tree, so git lists it", "repo", "repo/.git", null, true, 1, [A]);
        Add("a .git file at the root is a linked work tree, so git lists it", "repo", null, "repo/.git", true, 1, [A]);
        Add("a .git directory above the root is a work tree holding it, so git lists it", "repo/inner", "repo/.git", null, true, 1, [A]);
        Add("a git that cannot be started is git absent from the path, so the tree is walked", "repo", "repo/.git", null, false, 1, [A, B]);
    }

    private const string A = "src/a/.spec/README.md";

    private const string B = "src/b/.spec/README.md";
}
