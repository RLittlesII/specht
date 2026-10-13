using System.IO.Abstractions;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>Loads the manifest and the pinned schemas, or the verified local upstream copy (0001-F7 B-024, B-028).</summary>
/// <param name="fileSystem">The file system the manifest is read through.</param>
/// <param name="store">The reader of local frontmatter schema bytes.</param>
/// <param name="copy">The verifier of the recorded hashes.</param>
public sealed class SpecSchemasLoader(IFileSystem fileSystem, FrontmatterSchemaStore store, UpstreamSchemaCopy copy)
{
    /// <summary>Loads the manifest and resolves its frontmatter schemas using the embedded version set.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The schemas and the structure the manifest declares.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse.</exception>
    /// <exception cref="SpechtManifestException">The manifest is rejected.</exception>
    public SpecSchemas Load(string root) => Load(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the manifest under <paramref name="root"/> and the schemas of the version it pins from
    /// <paramref name="versions"/> (<c>0001-F7</c> B-001, C-4).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="versions">The version set the pin selects from.</param>
    /// <returns>The schemas and the structure the manifest declares.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse.</exception>
    /// <exception cref="SpechtManifestException">The manifest or its upstream copy is rejected, or the pin is not shipped.</exception>
    public SpecSchemas Load(string root, SchemaVersions versions)
    {
        var embedded = SpecSchemas.Load(fileSystem, root, versions);
        var source = embedded.Structure.SpechtSchema;
        if (source is null)
        {
            return embedded;
        }

        var files = source.Files.ToDictionary(static file => file.Kind, file => store.Read(root, file.FileName), StringComparer.Ordinal);
        var texts = copy.Verify(source, files);
        var version = embedded.Version with
        {
            FeatureSchema = texts["feature"],
            ItemSchema = texts["task"],
            EpicSchema = texts.GetValueOrDefault("epic", embedded.Version.EpicSchema),
        };

        return new SpecSchemas(version, embedded.Structure, SpecSchemaSource.Upstream);
    }
}
