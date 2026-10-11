using System.Globalization;
using System.Text.Json;
using Json.Schema;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC001-SPEC005 - every specification, item and epic file carries
/// frontmatter that satisfies its schema in <c>.spec/schema/</c>.
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
            foreach (var violation in Check(feature.RelativePath, feature.Document.Frontmatter, model.Schemas.Feature, "SPEC002"))
            {
                yield return violation;
            }
        }

        foreach (var item in model.Items)
        {
            foreach (var violation in Check(item.RelativePath, item.Frontmatter, model.Schemas.Item, "SPEC003"))
            {
                yield return violation;
            }
        }

        foreach (var epic in model.Epics)
        {
            foreach (var violation in Check(epic.RelativePath, epic.Frontmatter, model.Schemas.Epic, "SPEC004"))
            {
                yield return violation;
            }
        }
    }

    private static IEnumerable<SpecViolation> Check(string path, Frontmatter frontmatter, JsonSchema schema, string ruleId)
    {
        if (frontmatter.Node is null)
        {
            yield return new SpecViolation(
                "SPEC001",
                SpecSeverity.Error,
                path,
                1,
                null,
                "no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping");

            yield break;
        }

        var instance = JsonSerializer.SerializeToElement(frontmatter.Node);
        var result = EvaluateInvariant(schema, instance);

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
                Describe(detail, key));
        }
    }

    private static EvaluationResults EvaluateInvariant(JsonSchema schema, JsonElement instance)
    {
        var culture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            return schema.Evaluate(instance, SpecSchemas.Options);
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
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
}
