using System.IO.Abstractions;

namespace Specht.Benchmarks;

/// <summary>Persists a <see cref="SpechtTree"/> through a file system (<c>0109-F1</c> B-016; C-9).</summary>
/// <param name="fileSystem">The file system a tree is written through.</param>
public sealed class SpechtTreeStore(IFileSystem fileSystem)
{
    /// <summary>Writes each file of <paramref name="tree"/> at its relative path under <paramref name="root"/>.</summary>
    /// <param name="tree">The tree to write.</param>
    /// <param name="root">The directory the tree's paths are relative to, which the caller creates and deletes.</param>
    public void Save(SpechtTree tree, string root)
    {
        foreach (var file in tree.Files)
        {
            var path = fileSystem.Path.Combine(root, file.RelativePath.Replace('/', fileSystem.Path.DirectorySeparatorChar));

            fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);

            using var stream = fileSystem.File.Create(path);

            stream.Write(file.Content.Span);
        }
    }
}
