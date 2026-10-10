using System.IO.Abstractions;
using Json.Schema;
using Specht.Manifest;

namespace Specht;

/// <summary>
/// Loads the schema files that define a specification's shape.
/// </summary>
/// <remarks>
/// The schemas are data, not code, so that whoever is authoring a
/// specification can read the contract they are being held to. The section
/// manifest lives beside them for the same reason - a C# string array would be
/// a second, invisible home for the twelve-section rule.
/// </remarks>
public sealed class SpecSchemas
{
    private SpecSchemas(SchemaVersion version, SpecStructure structure)
    {
        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };

        Feature = JsonSchema.FromText(version.FeatureSchema, options);
        Item = JsonSchema.FromText(version.ItemSchema, options);
        Epic = JsonSchema.FromText(version.EpicSchema, options);
        Structure = structure;
        Version = version;
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

    /// <summary>The ordered section contract and id grammars.</summary>
    public SpecStructure Structure { get; }

    /// <summary>The schema version these schemas were built from, and whose rule ids a check over them evaluates (ADR-0008).</summary>
    public SchemaVersion Version { get; }

    /// <summary>
    /// Loads every schema from <paramref name="root"/>'s <c>.spec/schema/</c>, the manifest first, each frontmatter schema
    /// from the file the manifest names for its kind (<c>0001-F5</c> B-008).
    /// </summary>
    /// <remarks>
    /// The check loads through a version set instead; this reads the on-disk frontmatter schemas, which no check selects
    /// until the schema source exists (<c>0001-F7</c> B-009). The version kept is the embedded one the manifest pins,
    /// carrying the texts read here in place of its own, so its rule ids are that version's (ADR-0008).
    /// Each load gets its own <see cref="SchemaRegistry"/>. The library's
    /// default registry is process-wide and refuses to re-register a
    /// <c>$id</c>, so a second load in one process - two roots in one test
    /// run, say - would throw rather than simply reading the schemas again.
    /// </remarks>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; no frontmatter schema is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; no frontmatter schema is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; no frontmatter schema is read.</exception>
    /// <exception cref="SpechtManifestException">
    /// The manifest is rejected, or pins a version the tool does not ship; no frontmatter schema is read.
    /// </exception>
    public static SpecSchemas Load(IFileSystem fileSystem, string root)
    {
        var structure = SpecManifest.Load(fileSystem, root);
        var version = SchemaVersions.Embedded.Select(structure.SchemaVersion);
        var directory = fileSystem.Path.Combine(root, ".spec", "schema");

        return new SpecSchemas(
            version with
            {
                FeatureSchema = Read(fileSystem, directory, structure.FrontmatterSchemas["feature"]),
                ItemSchema = Read(fileSystem, directory, structure.FrontmatterSchemas["task"]),
                EpicSchema = Read(fileSystem, directory, structure.FrontmatterSchemas["epic"]),
            },
            structure);
    }

    /// <summary>
    /// Loads the manifest from <paramref name="root"/>'s <c>.spec/schema/</c> and the frontmatter schemas of the version it
    /// pins from <paramref name="versions"/> (<c>0001-F7</c> B-001, C-4).
    /// </summary>
    /// <remarks>Each load gets its own <see cref="SchemaRegistry"/>, for the same reason as the on-disk load.</remarks>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; no frontmatter schema is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; no frontmatter schema is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; no frontmatter schema is read.</exception>
    /// <exception cref="SpechtManifestException">
    /// The manifest is rejected, or pins a version <paramref name="versions"/> does not hold; no frontmatter schema is read.
    /// </exception>
    public static SpecSchemas Load(IFileSystem fileSystem, string root, SchemaVersions versions)
    {
        var structure = SpecManifest.Load(fileSystem, root);

        return new SpecSchemas(versions.Select(structure.SchemaVersion), structure);
    }

    private static string Read(IFileSystem fileSystem, string directory, string name) =>
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(directory, name));
}
