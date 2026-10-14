using System.IO.Abstractions;
using Json.Schema;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>
/// Loads the schema files that define a specification's shape.
/// </summary>
/// <remarks>
/// The schemas are data, not code, so that whoever is authoring a
/// specification can read the contract they are being held to. The section
/// manifest is data for the same reason - a C# string array would be
/// a second, invisible home for the twelve-section rule.
/// </remarks>
public sealed class SpecSchemas
{
    internal SpecSchemas(SchemaVersion version, SpecStructure structure, SpecSchemaSource source = SpecSchemaSource.Embedded)
    {
        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };

        Feature = JsonSchema.FromText(version.FeatureSchema, options);
        Item = JsonSchema.FromText(version.ItemSchema, options);
        Epic = JsonSchema.FromText(version.EpicSchema, options);
        Structure = structure;
        Version = version;
        Source = source;
    }

    /// <summary>Evaluation options that make <c>format</c> an assertion rather than an annotation.</summary>
    public static EvaluationOptions Options { get; } = new()
    {
        RequireFormatValidation = true,
        OutputFormat = OutputFormat.List,
    };

    /// <summary>Schema for a Feature specification's frontmatter.</summary>
    public JsonSchema Feature { get; }

    /// <summary>Schema for a task, test, bug or spike file's frontmatter.</summary>
    public JsonSchema Item { get; }

    /// <summary>Schema for an epic file's frontmatter.</summary>
    public JsonSchema Epic { get; }

    /// <summary>Where the frontmatter schemas used by the check came from.</summary>
    public SpecSchemaSource Source { get; }

    /// <summary>The ordered section contract and id grammars.</summary>
    public SpecStructure Structure { get; }

    /// <summary>The schema version these schemas were built from, and whose rule ids a check over them evaluates (ADR-0008).</summary>
    public SchemaVersion Version { get; }

    /// <summary>Compiles the named local schemas after the caller validates the manifest and resolves its pin.</summary>
    public static SpecSchemas Load(IFileSystem fileSystem, string root, SpecStructure structure, SchemaVersion version)
    {
        var directory = fileSystem.Path.Combine([root, .. SpecManifestStore.FrontmatterSchemaFolder.Split('/')]);
        return new SpecSchemas(
            version with
            {
                FeatureSchema = Read(fileSystem, directory, structure.FrontmatterSchemas["feature"]),
                ItemSchema = Read(fileSystem, directory, structure.FrontmatterSchemas["task"]),
                EpicSchema = Read(fileSystem, directory, structure.FrontmatterSchemas["epic"]),
            }, structure);
    }

    /// <summary>Compiles the selected version after the caller validates the manifest and resolves its pin.</summary>
    public static SpecSchemas Load(SpecStructure structure, SchemaVersion version) => new(version, structure);

    private static string Read(IFileSystem fileSystem, string directory, string name) =>
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(directory, name));
}
