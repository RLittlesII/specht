using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// The frontmatter schema evaluation SPEC002, SPEC003 and SPEC004 share: the kind's schema in
/// <c>.specht/schema/frontmatter/</c>, evaluated under the invariant culture, each failure of the walk reported at its key
/// with its message and what the schema expected there (ADR-0010 § "One rule per id").
/// </summary>
/// <implements epic="0001" feature="F5" claim="B-008" />
public sealed class FrontmatterSchemaCheck
{
    /// <summary>Parses the manifest's exact kind names without reading a document or root.</summary>
    /// <param name="kind">The kind as the caller typed it.</param>
    /// <returns>The kind.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public FrontmatterKind ParseKind(string kind) => kind switch
    {
        "feature" => FrontmatterKind.Feature,
        "task" => FrontmatterKind.Task,
        "epic" => FrontmatterKind.Epic,
        _ => throw new ArgumentException($"Unknown frontmatter kind '{kind}'. Available kinds: feature, task, epic.", nameof(kind)),
    };

    /// <summary>The kind's schema file, relative to the root, as the manifest names it under every source.</summary>
    /// <param name="schemas">The selected schemas.</param>
    /// <param name="kind">The kind.</param>
    /// <returns>The schema file's repository-relative path.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public string SchemaFile(SpechtSchemas schemas, FrontmatterKind kind) =>
        SpechtManifestStore.FrontmatterSchemaFolder + "/" + schemas.Structure.FrontmatterSchemas[Select(schemas, kind).Key];

    /// <summary>The keys the kind's schema requires at its root, in the schema's order.</summary>
    /// <param name="schemas">The selected schemas.</param>
    /// <param name="kind">The kind.</param>
    /// <returns>The required keys.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public JsonArray Required(SpechtSchemas schemas, FrontmatterKind kind) => Required(Select(schemas, kind).Schema);

    /// <summary>
    /// Evaluates one document's frontmatter against its kind's schema and finds each failure; a document with no
    /// frontmatter is SPEC001's and finds nothing here.
    /// </summary>
    /// <param name="path">The document's repository-relative path.</param>
    /// <param name="frontmatter">The document's frontmatter.</param>
    /// <param name="schemas">The selected schemas.</param>
    /// <param name="kind">The kind whose schema is evaluated.</param>
    /// <returns>The failures, in the order the walk yields them.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtFinding> Evaluate(
        string path, Frontmatter frontmatter, SpechtSchemas schemas, FrontmatterKind kind)
    {
        var schema = Select(schemas, kind).Schema;
        var schemaFile = SchemaFile(schemas, kind);

        if (frontmatter.Node is null)
        {
            yield break;
        }

        var instance = JsonSerializer.SerializeToElement(frontmatter.Node);
        var result = EvaluateInvariant(
            schema, instance, schemas.EvaluationOptions, schemas.Source == SpechtSchemaSource.Embedded ? null : schemaFile);

        if (result.IsValid)
        {
            yield break;
        }

        foreach (var detail in Failures(result))
        {
            var key = KeyOf(detail);

            yield return new SpechtFinding(
                path,
                frontmatter.LineOf(key),
                key,
                Describe(detail, key))
            {
                Expected = Expectation(schemaFile, key, Constraints(schema, detail)),
            };
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static (JsonSchema Schema, string Key) Select(SpechtSchemas schemas, FrontmatterKind kind) => kind switch
    {
        FrontmatterKind.Feature => (schemas.Feature, "feature"),
        FrontmatterKind.Task => (schemas.Item, "task"),
        FrontmatterKind.Epic => (schemas.Epic, "epic"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The kind must be feature, task or epic."),
    };

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static EvaluationResults EvaluateInvariant(
        JsonSchema schema, JsonElement instance, EvaluationOptions options, string? schemaPath)
    {
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            return schema.Evaluate(instance, options);
        }
        catch (RefResolutionException) when (schemaPath is not null)
        {
            throw new SpechtManifestException($"{schemaPath}: the selected local schema contains a reference that could not be resolved.");
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static JsonArray Required(JsonSchema schema) =>
        schema.Root.Source.ValueKind == JsonValueKind.Object
        && schema.Root.Source.TryGetProperty("required", out var required)
        && required.ValueKind == JsonValueKind.Array
            ? new([.. required.EnumerateArray().Select(static name => JsonNode.Parse(name.GetRawText()))])
            : [];

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static JsonObject Expectation(string schemaFile, string? key, JsonObject constraints)
    {
        var expected = new JsonObject { ["schema"] = schemaFile };

        if (key is not null)
        {
            expected["key"] = key;
        }

        expected["constraints"] = constraints;

        return expected;
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static JsonObject Constraints(JsonSchema schema, EvaluationResults detail)
    {
        var constraints = new JsonObject();

        if (detail.Errors is null || Place(schema.Root.Source, detail.EvaluationPath.ToString()) is not { } place)
        {
            return constraints;
        }

        foreach (var keyword in place.EnumerateObject())
        {
            if (Assertions.Contains(keyword.Name) && detail.Errors.ContainsKey(keyword.Name))
            {
                constraints[keyword.Name] = JsonNode.Parse(keyword.Value.GetRawText());
            }
        }

        return constraints;
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static JsonElement? Place(JsonElement root, string evaluationPath)
    {
        JsonElement? place = root;
        var argument = false;

        foreach (var name in Segments(evaluationPath))
        {
            if (!argument && Conditions.Contains(name))
            {
                return null;
            }

            place = !argument && name == "$ref" ? Reference(root, place.Value) : Step(place.Value, name);
            argument = !argument && Named.Contains(name);

            if (place is null)
            {
                return null;
            }
        }

        return place is { ValueKind: JsonValueKind.Object } ? place : null;
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static JsonElement? Reference(JsonElement root, JsonElement place)
    {
        if (place.ValueKind != JsonValueKind.Object
            || !place.TryGetProperty("$ref", out var target)
            || target.GetString() is not { } reference
            || (reference != "#" && !reference.StartsWith("#/", StringComparison.Ordinal)))
        {
            return null;
        }

        JsonElement? resolved = root;

        foreach (var name in Segments(Uri.UnescapeDataString(reference[1..])))
        {
            resolved = Step(resolved.Value, name);

            if (resolved is null)
            {
                return null;
            }
        }

        return resolved;
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static JsonElement? Step(JsonElement place, string name)
    {
        if (place.ValueKind == JsonValueKind.Object && place.TryGetProperty(name, out var member))
        {
            return member;
        }

        return place.ValueKind == JsonValueKind.Array
            && int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
            && index < place.GetArrayLength()
                ? place[index]
                : null;
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static IEnumerable<string> Segments(string pointer) =>
        pointer.Split('/').Skip(1).Select(static segment =>
            segment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal));

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static IEnumerable<EvaluationResults> Failures(EvaluationResults results)
    {
        if (!results.IsValid && results.Errors is { Count: > 0 })
        {
            yield return results;
        }

        if (results.Details is null)
        {
            yield break;
        }

        foreach (var nested in results.Details)
        {
            if (nested.IsValid)
            {
                continue;
            }

            foreach (var failure in Failures(nested))
            {
                yield return failure;
            }
        }
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static string? KeyOf(EvaluationResults results)
    {
        var segments = results.InstanceLocation.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);

        return segments.Length == 0 ? null : segments[0];
    }

    /// <implements epic="0001" feature="F5" claim="B-008" />
    private static string Describe(EvaluationResults results, string? key)
    {
        var errors = results.Errors is null
            ? "does not satisfy the schema"
            : string.Join("; ", results.Errors.Values);

        var location = results.InstanceLocation.ToString();

        return key is null
            ? $"frontmatter {errors}"
            : $"frontmatter '{location.TrimStart('/')}' {errors}";
    }

    private static readonly HashSet<string> Assertions = new(StringComparer.Ordinal)
    {
        "type", "enum", "const", "multipleOf", "maximum", "exclusiveMaximum", "minimum", "exclusiveMinimum", "maxLength",
        "minLength", "pattern", "maxItems", "minItems", "uniqueItems", "maxContains", "minContains", "maxProperties",
        "minProperties", "required", "dependentRequired", "format",
    };

    private static readonly HashSet<string> Conditions = new(StringComparer.Ordinal) { "if", "not", "anyOf", "oneOf" };

    private static readonly HashSet<string> Named =
        new(StringComparer.Ordinal) { "properties", "patternProperties", "$defs", "dependentSchemas", "allOf", "prefixItems" };
}
