using Json.Schema;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>The compiled schemas that define a specification's shape, as <see cref="SpechtSchemaCompiler"/> builds them.</summary>
/// <remarks>
/// The schemas are data, not code, so that whoever is authoring a
/// specification can read the contract they are being held to. The section
/// manifest is data for the same reason - a C# string array would be
/// a second, invisible home for the twelve-section rule.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtSchemas
{
    /// <implements epic="0001" feature="F1" claim="B-004" />
    internal SpechtSchemas(
        SchemaVersion version,
        SpechtStructure structure,
        SpechtSchemaSource source,
        JsonSchema feature,
        JsonSchema item,
        JsonSchema epic,
        EvaluationOptions evaluationOptions)
    {
        Feature = feature;
        Item = item;
        Epic = epic;
        Structure = structure;
        Version = version;
        Source = source;
        EvaluationOptions = evaluationOptions;
    }

    /// <summary>Schema for a Feature specification's frontmatter.</summary>
    public JsonSchema Feature { get; }

    /// <summary>Schema for a task, test, bug or spike file's frontmatter.</summary>
    public JsonSchema Item { get; }

    /// <summary>Schema for an epic file's frontmatter.</summary>
    public JsonSchema Epic { get; }

    /// <summary>Where the frontmatter schemas used by the check came from.</summary>
    public SpechtSchemaSource Source { get; }

    /// <summary>The ordered section contract and id grammars.</summary>
    public SpechtStructure Structure { get; }

    /// <summary>The schema version these schemas were built from, and whose rule ids a check over them evaluates (ADR-0008).</summary>
    public SchemaVersion Version { get; }

    /// <summary>The options the frontmatter rules evaluate these schemas with: <c>format</c> is an assertion, not an annotation.</summary>
    public EvaluationOptions EvaluationOptions { get; }
}
