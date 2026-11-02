namespace Specht.Tree;

/// <summary>A specification tree held in memory, its files relative to a root it does not name (<c>0109-F1</c> B-016).</summary>
/// <param name="files">The tree's files, in the order they are written.</param>
/// <implements epic="0109" feature="F1" claim="B-016" />
public sealed class SpechtTree(IReadOnlyList<SpechtTreeFile> files)
{
    /// <summary>Gets the tree's files, in the order they were given.</summary>
    public IReadOnlyList<SpechtTreeFile> Files { get; } = files;
}
