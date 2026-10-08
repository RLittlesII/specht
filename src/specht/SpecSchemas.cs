using System.Text.Json;
using Json.Schema;

namespace specht;

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
    private SpecSchemas(JsonSchema feature, JsonSchema item, JsonSchema epic, SpecStructure structure)
    {
        Feature = feature;
        Item = item;
        Epic = epic;
        Structure = structure;
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

    /// <summary>Loads every schema from <paramref name="root"/>'s <c>.spec/schema/</c>.</summary>
    /// <remarks>
    /// Each load gets its own <see cref="SchemaRegistry"/>. The library's
    /// default registry is process-wide and refuses to re-register a
    /// <c>$id</c>, so a second load in one process - two roots in one test
    /// run, say - would throw rather than simply reading the schemas again.
    /// </remarks>
    public static SpecSchemas Load(string root)
    {
        var directory = Path.Combine(root, ".spec", "schema");
        var options = new BuildOptions { SchemaRegistry = new SchemaRegistry() };

        return new SpecSchemas(
            Read(directory, "feature-spec.frontmatter.schema.json", options),
            Read(directory, "task.frontmatter.schema.json", options),
            Read(directory, "epic.frontmatter.schema.json", options),
            ReadStructure(Path.Combine(directory, "spec-structure.schema.json")));
    }

    private static JsonSchema Read(string directory, string name, BuildOptions options) =>
        JsonSchema.FromText(File.ReadAllText(Path.Combine(directory, name)), options);

    private static SpecStructure ReadStructure(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var sections = new List<string>();

        foreach (var element in root.GetProperty("sections").EnumerateArray())
        {
            sections.Add(element.GetString()!);
        }

        var tables = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

        foreach (var entry in root.GetProperty("tables").EnumerateObject())
        {
            var headers = new List<string>();

            foreach (var element in entry.Value.EnumerateArray())
            {
                headers.Add(element.GetString()!);
            }

            tables[entry.Name] = headers;
        }

        var identifiers = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var entry in root.GetProperty("identifiers").EnumerateObject())
        {
            identifiers[entry.Name] = entry.Value.GetString()!;
        }

        return new SpecStructure(sections, tables, identifiers);
    }
}
