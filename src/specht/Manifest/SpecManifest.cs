using System.IO.Abstractions;
using System.Text.Json;
using Specht.Resources;

namespace Specht.Manifest;

/// <summary>
/// Reads the manifest, <c>.specht/manifest.json</c>, whole: every key is checked before a structure is
/// returned, so no rule ever sees a manifest that was not accepted (<c>0001-F5</c> C-5).
/// </summary>
/// <remarks>
/// The manifest is always read from the root's file (C-6). The default manifest (A-4) is this repository's own manifest,
/// embedded into the engine from that same file rather than copied, and supplies only the values a manifest leaves out
/// (B-019); it never stands in for a missing file.
/// </remarks>
public static class SpecManifest
{
    /// <summary>The folder under a root that holds the tool's configuration (ADR-0011).</summary>
    public const string ConfigurationFolder = ".specht";

    /// <summary>The folder under a root that holds the frontmatter schemas (<c>0001-F5</c> B-008).</summary>
    public const string FrontmatterSchemaFolder = ConfigurationFolder + "/schema/frontmatter";

    /// <summary>The manifest's path under a root, as a message names it.</summary>
    public const string RelativePath = ConfigurationFolder + "/manifest.json";

    /// <summary>The path under a root where the manifest was read before ADR-0011, and is not read (<c>0001-F2</c> A-3).</summary>
    public const string RetiredRelativePath = ".spec/schema/spec-structure.schema.json";

    /// <summary>Reads, checks, and fills the manifest under <paramref name="root"/>.</summary>
    /// <param name="fileSystem">The file system the manifest is read through.</param>
    /// <param name="root">The repository root.</param>
    /// <returns>
    /// The section contract, id grammars, discovery inputs and schema file names, every omitted value read as the default
    /// manifest, and the rule settings as the manifest wrote them (<c>0001-F5</c> B-044).
    /// </returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no file at the manifest path.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest is not well-formed JSON or not the manifest's shape.</exception>
    /// <exception cref="SpechtManifestException">
    /// The manifest carries a key the engine does not know, a <c>schemaVersion</c> that is not a <c>major.minor.patch</c> string, a
    /// role naming a title <c>sections</c> does not list, a <c>tables</c> key that is not a role, a marker whose text is empty,
    /// an empty <c>taskFiles</c>, <c>epicFiles</c> or <c>companionFiles</c> list, an <c>exclusions</c> entry with a
    /// <c>/</c> inside it and no leading <c>/</c>, a <c>layouts</c> entry whose <c>identity</c> holds a member that is not a
    /// non-negative integer, a member other than <c>epic</c> and <c>feature</c>, or neither of the two, a <c>rules</c> entry
    /// whose value is not <c>error</c>, <c>warning</c> or <c>off</c>, or a <c>spechtSchema</c> that lacks a member, gives a
    /// <c>location</c> that is not an <c>https</c> address or a <c>version</c> that is not a non-empty string, or holds for a
    /// fetched kind no hash or one that is not <c>sha256:</c> and 64 lowercase hexadecimal digits (<c>0001-F7</c> B-069 to B-073).
    /// </exception>
    public static SpecStructure Load(IFileSystem fileSystem, string root)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        var path = Under(fileSystem, root, RelativePath);

        if (!fileSystem.File.Exists(path))
        {
            throw new SpechtManifestNotFoundException(fileSystem.File.Exists(Under(fileSystem, root, RetiredRelativePath)));
        }

        try
        {
            return new SpecManifestParser(new SpechtResources().DefaultManifest).Parse(fileSystem.File.ReadAllText(path));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
    }

    /// <summary>Resolves a root-relative path with <c>/</c> separators under <paramref name="root"/>.</summary>
    /// <param name="fileSystem">The file system whose separators the result carries.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="relative">The path under the root, such as <see cref="FrontmatterSchemaFolder"/>.</param>
    /// <returns>The path under <paramref name="root"/>.</returns>
    public static string Under(IFileSystem fileSystem, string root, string relative) =>
        fileSystem.Path.Combine([root, .. relative.Split('/')]);

}
