using System.IO.Abstractions;

namespace Specht.Tree;

/// <summary>Persists a <see cref="SpechtTree"/> through a file system, and loads one (<c>0109-F1</c> B-016; C-9; ADR-0010 § "I/O is confined").</summary>
/// <param name="fileSystem">The file system a tree is written and read through.</param>
/// <implements epic="0109" feature="F1" claim="B-016" />
public sealed class SpechtTreeStore(IFileSystem fileSystem)
{
    /// <summary>Writes each file of <paramref name="tree"/> at its relative path under <paramref name="root"/>.</summary>
    /// <param name="tree">The tree to write.</param>
    /// <param name="root">The directory the tree's paths are relative to, which the caller creates and deletes.</param>
    /// <implements epic="0109" feature="F1" claim="B-016" />
    public void Save(SpechtTree tree, string root)
    {
        foreach (var file in tree.Files)
        {
            var path = Under(root, file.RelativePath);

            fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);

            using var stream = fileSystem.File.Create(path);

            stream.Write(file.Content.Span);
        }
    }

    /// <summary>
    /// Reads exactly the files <paramref name="relativePaths"/> names under <paramref name="root"/>, in that order, and
    /// nothing else (item <c>0166</c>; ADR-0010 § "I/O is confined").
    /// </summary>
    /// <param name="root">The directory the paths are relative to.</param>
    /// <param name="relativePaths">The root-relative paths to read, their segments separated by <c>/</c>.</param>
    /// <returns>A tree of those files' bytes, in the order given.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtTree Load(string root, IReadOnlyList<string> relativePaths) =>
        new(relativePaths.Select(relative => new SpechtTreeFile(relative, fileSystem.File.ReadAllBytes(Under(root, relative)))).ToArray());

    /// <implements epic="0109" feature="F1" claim="B-016" />
    private string Under(string root, string relative) =>
        fileSystem.Path.Combine(root, relative.Replace('/', fileSystem.Path.DirectorySeparatorChar));
}
