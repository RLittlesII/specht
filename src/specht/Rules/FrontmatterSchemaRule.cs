using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC001-SPEC005 - every specification, item and epic file carries
/// frontmatter that satisfies its schema in <c>.specht/schema/frontmatter/</c>.
/// </summary>
/// <remarks>
/// SPEC005 - a linked specification records when it last mirrored - is
/// expressed inside the Feature schema as an <c>if</c>/<c>then</c>, because
/// "github_issue set implies synced_at present" is a statement about one
/// file's frontmatter and belongs with the rest of them.
/// </remarks>
public sealed class FrontmatterSchemaRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC001";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC001", "SPEC002", "SPEC003", "SPEC004"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        foreach (var feature in model.Features)
        {
            foreach (var violation in Check(feature.RelativePath, feature.Document.Frontmatter, model.Schemas.Feature, "SPEC002", model.Schemas, "feature"))
            {
                yield return violation;
            }
        }

        foreach (var item in model.Items)
        {
            foreach (var violation in Check(item.RelativePath, item.Frontmatter, model.Schemas.Item, "SPEC003", model.Schemas, "task"))
            {
                yield return violation;
            }
        }

        foreach (var epic in model.Epics)
        {
            foreach (var violation in Check(epic.RelativePath, epic.Frontmatter, model.Schemas.Epic, "SPEC004", model.Schemas, "epic"))
            {
                yield return violation;
            }
        }
    }

    private static IEnumerable<SpecViolation> Check(
        string path, Frontmatter frontmatter, JsonSchema schema, string ruleId, SpecSchemas schemas, string kind)
    {
        var schemaFile = SpecManifestStore.FrontmatterSchemaFolder + "/" + schemas.Structure.FrontmatterSchemas[kind];

        if (frontmatter.Node is null)
        {
            yield return new SpecViolation(
                "SPEC001",
                SpecSeverity.Error,
                path,
                1,
                null,
                "no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping")
            {
                Expected = new JsonObject { ["schema"] = schemaFile, ["required"] = Required(schema) },
            };

            yield break;
        }

        var instance = JsonSerializer.SerializeToElement(frontmatter.Node);
        var result = EvaluateInvariant(schema, instance, schemas.Source == SpecSchemaSource.Embedded ? null : schemaFile);

        if (result.IsValid)
        {
            yield break;
        }

        foreach (var detail in Failures(result))
        {
            var key = KeyOf(detail);

            yield return new SpecViolation(
                ruleId,
                SpecSeverity.Error,
                path,
                frontmatter.LineOf(key),
                key,
                Describe(detail, key))
            {
                Expected = Expectation(schemaFile, key, Constraints(schema, detail)),
            };
        }
    }

    private static EvaluationResults EvaluateInvariant(JsonSchema schema, JsonElement instance, string? schemaPath)
    {
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            return schema.Evaluate(instance, SpecSchemas.Options);
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

    private static JsonArray Required(JsonSchema schema) =>
        schema.Root.Source.ValueKind == JsonValueKind.Object
        && schema.Root.Source.TryGetProperty("required", out var required)
        && required.ValueKind == JsonValueKind.Array
            ? new([.. required.EnumerateArray().Select(static name => JsonNode.Parse(name.GetRawText()))])
            : [];

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

    private static JsonElement? Place(JsonElement root, string evaluationPath)
    {
        var place = root;
        var argument = false;

        foreach (var segment in evaluationPath.Split('/').Skip(1))
        {
            var name = segment.Replace("~1", "/", StringComparison.Ordinal).Replace("~0", "~", StringComparison.Ordinal);

            if (!argument && Conditions.Contains(name))
            {
                return null;
            }

            argument = !argument && Named.Contains(name);

            if (place.ValueKind == JsonValueKind.Object && place.TryGetProperty(name, out var member))
            {
                place = member;
            }
            else if (place.ValueKind == JsonValueKind.Array
                && int.TryParse(name, NumberStyles.None, CultureInfo.InvariantCulture, out var index)
                && index < place.GetArrayLength())
            {
                place = place[index];
            }
            else
            {
                return null;
            }
        }

        return place.ValueKind == JsonValueKind.Object ? place : null;
    }

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

    private static string? KeyOf(EvaluationResults results)
    {
        var segments = results.InstanceLocation.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);

        return segments.Length == 0 ? null : segments[0];
    }

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
