using System.IO.Abstractions;
using System.Text.Json;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>Sequences manifest validation and schema loading until ADR-0010 D2 supplies the model reader.</summary>
/// <param name="fileSystem">The file system for the retained on-disk schema path.</param>
/// <param name="store">The manifest and local schema store.</param>
/// <param name="parser">The pure manifest parser and pin resolver.</param>
/// <param name="copy">The verifier of local upstream bytes.</param>
public sealed class SpecSchemasLoader(
    IFileSystem fileSystem, SpecManifestStore store, SpecManifestParser parser, UpstreamSchemaCopy copy)
{
    /// <summary>Loads embedded schemas or the verified local upstream copy after manifest validation.</summary>
    public SpecSchemas Load(string root)
    {
        var structure = LoadManifest(root);
        var selected = parser.PinnedVersion(structure);
        var embedded = SpecSchemas.Load(structure, selected);
        var source = structure.SpechtSchema;
        if (source is null)
        {
            return embedded;
        }

        var files = source.Files.ToDictionary(
            static file => file.Kind, file => store.ReadFrontmatterSchema(root, file.FileName), StringComparer.Ordinal);
        var texts = copy.Verify(source, files);
        var version = selected with
        {
            FeatureSchema = texts["feature"],
            ItemSchema = texts["task"],
            EpicSchema = texts.GetValueOrDefault("epic", selected.EpicSchema),
        };
        return new SpecSchemas(version, structure, SpecSchemaSource.Upstream);
    }

    /// <summary>Loads the explicitly requested local schema files after the same manifest validation.</summary>
    public SpecSchemas LoadOnDisk(string root)
    {
        var structure = LoadManifest(root);
        return SpecSchemas.Load(fileSystem, root, structure, parser.PinnedVersion(structure));
    }

    /// <summary>Reads and parses a manifest, retaining the existing root, missing-file and shape failures.</summary>
    public SpecStructure LoadManifest(string root)
    {
        if (!store.RootExists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        if (!store.ManifestExists(root))
        {
            throw new SpechtManifestNotFoundException(store.RetiredManifestExists(root));
        }

        try
        {
            return parser.Parse(store.ReadManifest(root));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
    }
}
