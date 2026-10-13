using System.IO.Abstractions;

namespace Specht.Manifest;

/// <summary>Reads the bytes of a local frontmatter schema file (0001-F7 B-024, B-077).</summary>
/// <param name="fileSystem">The file system the local copy is read through.</param>
public sealed class FrontmatterSchemaStore(IFileSystem fileSystem)
{
    /// <summary>Reads a file in the root's frontmatter schema folder, or returns null when it is absent.</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="fileName">The schema file name from the manifest.</param>
    /// <returns>The file's unchanged bytes, or null.</returns>
    public byte[]? Read(string root, string fileName)
    {
        var directory = SpecManifest.Under(fileSystem, root, SpecManifest.FrontmatterSchemaFolder);
        var path = fileSystem.Path.Combine(directory, fileName);

        return fileSystem.File.Exists(path) ? fileSystem.File.ReadAllBytes(path) : null;
    }
}
