using System.IO.Abstractions;

namespace Specht.Manifest;

/// <summary>Reads manifest observations, text and local schema bytes without interpreting them (ADR-0010 D1).</summary>
/// <param name="fileSystem">The file system the store reads through.</param>
public sealed class SpecManifestStore(IFileSystem fileSystem)
{
    /// <summary>The configuration folder under a root (ADR-0011).</summary>
    public const string ConfigurationFolder = ".specht";

    /// <summary>The local frontmatter schema folder (0001-F5 B-008).</summary>
    public const string FrontmatterSchemaFolder = ConfigurationFolder + "/schema/frontmatter";

    /// <summary>The manifest's root-relative path.</summary>
    public const string RelativePath = ConfigurationFolder + "/manifest.json";

    /// <summary>The retired path, which is observed but never opened (0001-F2 B-038).</summary>
    public const string RetiredRelativePath = ".spec/schema/spec-structure.schema.json";

    /// <summary>Reports whether the root is a directory.</summary>
    public bool RootExists(string root) => fileSystem.Directory.Exists(root);

    /// <summary>Reports whether a manifest exists at the current path.</summary>
    public bool ManifestExists(string root) => fileSystem.File.Exists(Under(root, RelativePath));

    /// <summary>Reports whether a file exists at the retired path.</summary>
    public bool RetiredManifestExists(string root) => fileSystem.File.Exists(Under(root, RetiredRelativePath));

    /// <summary>Reads the current manifest's uninterpreted text.</summary>
    public string ReadManifest(string root) => fileSystem.File.ReadAllText(Under(root, RelativePath));

    /// <summary>Reads unchanged local schema bytes, or null when that file is absent (0001-F7 B-024, B-077).</summary>
    public byte[]? ReadFrontmatterSchema(string root, string fileName)
    {
        var path = fileSystem.Path.Combine(Under(root, FrontmatterSchemaFolder), fileName);
        return fileSystem.File.Exists(path) ? fileSystem.File.ReadAllBytes(path) : null;
    }

    private string Under(string root, string relative) => fileSystem.Path.Combine([root, .. relative.Split('/')]);
}
