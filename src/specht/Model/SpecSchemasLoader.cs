using System.Text;
using System.Text.Json;
using Json.Schema;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>Sequences manifest validation and schema loading until ADR-0010 D2 supplies the model reader.</summary>
/// <param name="store">The manifest and local schema store.</param>
/// <param name="parser">The pure manifest parser and pin resolver.</param>
/// <param name="copy">The verifier of local upstream bytes.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpecSchemasLoader(SpecManifestStore store, SpecManifestParser parser, UpstreamSchemaCopy copy)
{
    /// <summary>Loads embedded, explicitly selected disk, or verified local upstream schemas after manifest validation.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecSchemas Load(string root) => Load(root, null);

    /// <summary>Loads frontmatter schemas using this invocation's selection ahead of the valid manifest's selection.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecSchemas Load(string root, SchemaSourceSelection? selection)
    {
        var structure = LoadManifest(root);
        var selected = parser.PinnedVersion(structure);
        var embedded = SpecSchemas.Load(structure, selected);
        var source = structure.SpechtSchema;
        var effective = selection ?? structure.SchemaSource;
        if (source is null)
        {
            return effective == SchemaSourceSelection.Disk ? LoadDisk(root, embedded) : embedded;
        }

        if (effective == SchemaSourceSelection.Embedded)
        {
            throw new SpechtManifestException(
                $"{SpecManifestStore.RelativePath}: schemaSource 'embedded' contradicts the recorded spechtSchema source.");
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
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecSchemas LoadOnDisk(string root) => Load(root, SchemaSourceSelection.Disk);

    /// <summary>Reads and parses a manifest, retaining the existing root, missing-file and shape failures.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
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

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private SpecSchemas LoadDisk(string root, SpecSchemas embedded)
    {
        var structure = embedded.Structure;
        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };
        var feature = ReadDisk(root, structure.FrontmatterSchemas["feature"], options);
        var item = ReadDisk(root, structure.FrontmatterSchemas["task"], options);
        (string Text, JsonSchema Schema)? epic = structure.DeclaresEpicGrammar
            ? ReadDisk(root, structure.FrontmatterSchemas["epic"], options) : null;
        var version = embedded.Version with
        {
            FeatureSchema = feature.Text,
            ItemSchema = item.Text,
            EpicSchema = epic?.Text ?? embedded.Version.EpicSchema,
        };
        return new SpecSchemas(version, structure, SpecSchemaSource.Disk, feature.Schema, item.Schema, epic?.Schema ?? embedded.Epic);
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private (string Text, JsonSchema Schema) ReadDisk(string root, string name, BuildOptions options)
    {
        var path = SpecManifestStore.FrontmatterSchemaFolder + "/" + name;
        try
        {
            var bytes = store.ReadFrontmatterSchema(root, name);
            if (bytes is null)
            {
                throw new SpechtManifestException($"{path}: the selected disk schema is absent.");
            }

            var text = Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');
            return (text, JsonSchema.FromText(text, options));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpechtManifestException($"{path}: the selected disk schema could not be read.");
        }
        catch (Exception exception) when (
            exception is JsonSchemaException or RefResolutionException or JsonException or InvalidOperationException or ArgumentException or FormatException)
        {
            throw new SpechtManifestException($"{path}: the selected disk schema could not be compiled as JSON Schema.");
        }
    }
}
