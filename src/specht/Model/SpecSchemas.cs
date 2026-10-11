using System.IO.Abstractions;
using Json.Schema;
using LanguageExt;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

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
    /// <returns>
    /// The schemas, or the manifest's failure, a <see cref="SpechtFailureKind.ManifestRejected"/> one included when the
    /// manifest pins a version the tool does not ship or sets a rule id outside the pinned version's vocabulary
    /// (<c>0001-F5</c> B-013); on a failure no frontmatter schema is read.
    /// </returns>
    public static Either<SpechtFailure, SpecSchemas> Load(IFileSystem fileSystem, string root)
    {
        var directory = fileSystem.Path.Combine(root, ".spec", "schema");

        return
            from structure in SpecManifest.Load(fileSystem, root)
            from version in Pinned(SchemaVersions.Embedded, structure)
            select new SpecSchemas(
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
    /// <returns>
    /// The schemas, or the manifest's failure, a <see cref="SpechtFailureKind.ManifestRejected"/> one included when the
    /// manifest pins a version <paramref name="versions"/> does not hold or sets a rule id outside the pinned version's
    /// vocabulary (<c>0001-F5</c> B-013); on a failure no frontmatter schema is read.
    /// </returns>
    public static Either<SpechtFailure, SpecSchemas> Load(IFileSystem fileSystem, string root, SchemaVersions versions) =>
        from structure in SpecManifest.Load(fileSystem, root)
        from version in Pinned(versions, structure)
        select new SpecSchemas(version, structure);

    private static Either<SpechtFailure, SchemaVersion> Pinned(SchemaVersions versions, SpecStructure structure) =>
        versions.Select(structure.SchemaVersion).Bind(version => Held(version, structure));

    private static Either<SpechtFailure, SchemaVersion> Held(SchemaVersion version, SpecStructure structure)
    {
        var outside = structure.Rules.Keys.Where(id => !version.RuleIds.Contains(id)).Select(static id => $"'{id}'").ToList();

        return outside.Count > 0
            ? new SpechtFailure(
                SpechtFailureKind.ManifestRejected,
                $"{SpecManifest.RelativePath}: rules names the rule id {string.Join(", ", outside)}, "
                    + $"which schemaVersion {version.Number} does not hold.")
            : version;
    }

    private static string Read(IFileSystem fileSystem, string directory, string name) =>
        fileSystem.File.ReadAllText(fileSystem.Path.Combine(directory, name));
}
