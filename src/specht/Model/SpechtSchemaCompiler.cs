using System.Text.Json;
using Json.Schema;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>Compiles a schema version's frontmatter schema text into <see cref="SpechtSchemas"/>; it reads nothing.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtSchemaCompiler
{
    /// <summary>Compiles the three frontmatter schemas <paramref name="version"/> carries, as embedded or as a verified upstream copy.</summary>
    /// <param name="version">The version whose schema text is compiled.</param>
    /// <param name="structure">The validated manifest.</param>
    /// <param name="source">Where the schema text came from.</param>
    /// <returns>The compiled schemas.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtSchemas Compile(SchemaVersion version, SpechtStructure structure, SpechtSchemaSource source)
    {
        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };

        return new SpechtSchemas(
            version,
            structure,
            source,
            JsonSchema.FromText(version.FeatureSchema, options),
            JsonSchema.FromText(version.ItemSchema, options),
            JsonSchema.FromText(version.EpicSchema, options),
            _evaluationOptions);
    }

    /// <summary>
    /// Reads and compiles the selected local schema files one at a time, the Feature schema, then the task schema, then the
    /// epic schema where the manifest declares the epic grammar, so the first file that fails names the failure
    /// (<c>0001-F7</c> B-024, B-077). Where no epic grammar is declared, the epic schema of <paramref name="version"/> stays.
    /// </summary>
    /// <param name="version">The pinned version.</param>
    /// <param name="structure">The validated manifest, which names each file.</param>
    /// <param name="read">Returns the text of the schema file the manifest names, by that file name.</param>
    /// <returns>The compiled schemas, their source <see cref="SpechtSchemaSource.Disk"/>.</returns>
    /// <exception cref="SpechtManifestException">A file could not be compiled as JSON Schema.</exception>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtSchemas CompileDisk(SchemaVersion version, SpechtStructure structure, Func<string, string> read)
    {
        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };
        var (Text, Schema) = CompileDisk(structure.FrontmatterSchemas["feature"], read, options);
        var item = CompileDisk(structure.FrontmatterSchemas["task"], read, options);
        (string Text, JsonSchema Schema)? epic = structure.DeclaresEpicGrammar
            ? CompileDisk(structure.FrontmatterSchemas["epic"], read, options)
            : null;
        var compiled = version with
        {
            FeatureSchema = Text,
            ItemSchema = item.Text,
            EpicSchema = epic?.Text ?? version.EpicSchema,
        };

        return new SpechtSchemas(
            compiled,
            structure,
            SpechtSchemaSource.Disk,
            Schema,
            item.Schema,
            epic?.Schema ?? Compile(version, structure, SpechtSchemaSource.Embedded).Epic,
            _evaluationOptions);
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static (string Text, JsonSchema Schema) CompileDisk(string name, Func<string, string> read, BuildOptions options)
    {
        try
        {
            var text = read(name);
            return (text, JsonSchema.FromText(text, options));
        }
        catch (Exception exception) when (
            exception is JsonSchemaException or RefResolutionException or JsonException or InvalidOperationException or ArgumentException or FormatException)
        {
            var path = SpechtManifestStore.FrontmatterSchemaFolder + "/" + name;
            throw new SpechtManifestException($"{path}: the selected disk schema could not be compiled as JSON Schema.");
        }
    }

    private readonly EvaluationOptions _evaluationOptions = new()
    {
        RequireFormatValidation = true,
        OutputFormat = OutputFormat.List,
    };
}
