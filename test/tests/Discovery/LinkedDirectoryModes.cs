namespace Specht.Tests.Discovery;

/// <summary>
/// Why a row discovers as it does, whether the root is a git work tree, and whether its two links to directories are
/// committed or left untracked (<c>0001-F6</c> B-011).
/// </summary>
public sealed class LinkedDirectoryModes : TheoryData<string, bool, bool>
{
    public LinkedDirectoryModes()
    {
        Add("a root outside every work tree is walked", false, false);
        Add("git lists a work tree whose links are untracked", true, false);
        Add("git lists a work tree whose links are committed", true, true);
    }
}
