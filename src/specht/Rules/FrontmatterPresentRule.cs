using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC001 - every specification, item and epic file opens with YAML frontmatter. It evaluates no schema; the schema
/// names only what the violation expects.
/// </summary>
/// <param name="check">The frontmatter schema evaluation, for the kind's schema file and required keys.</param>
/// <implements epic="0001" feature="F5" claim="B-008" />
public sealed class FrontmatterPresentRule(FrontmatterSchemaCheck check) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC001";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC001"];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtViolation> Evaluate(SpechtModel model)
    {
        foreach (var feature in model.Features)
        {
            foreach (var violation in Evaluate(feature.RelativePath, feature.Document.Frontmatter, model.Schemas, FrontmatterKind.Feature))
            {
                yield return violation;
            }
        }

        foreach (var item in model.Items)
        {
            foreach (var violation in Evaluate(item.RelativePath, item.Frontmatter, model.Schemas, FrontmatterKind.Task))
            {
                yield return violation;
            }
        }

        foreach (var epic in model.Epics)
        {
            foreach (var violation in Evaluate(epic.RelativePath, epic.Frontmatter, model.Schemas, FrontmatterKind.Epic))
            {
                yield return violation;
            }
        }
    }

    /// <summary>Evaluates one document of <paramref name="kind"/>, as the tree's walk does each of its own.</summary>
    /// <param name="path">The document's repository-relative path.</param>
    /// <param name="frontmatter">The document's frontmatter.</param>
    /// <param name="schemas">The selected schemas.</param>
    /// <param name="kind">The document's kind.</param>
    /// <returns>The one violation when the document has no frontmatter, otherwise none.</returns>
    /// <implements epic="0001" feature="F5" claim="B-008" />
    public IEnumerable<SpechtViolation> Evaluate(string path, Frontmatter frontmatter, SpechtSchemas schemas, FrontmatterKind kind)
    {
        var schemaFile = check.SchemaFile(schemas, kind);

        if (frontmatter.Node is null)
        {
            yield return new SpechtViolation(
                "SPEC001",
                SpechtSeverity.Error,
                path,
                1,
                null,
                "no YAML frontmatter - every tracked specification artifact opens with a '---' delimited mapping")
            {
                Expected = new JsonObject { ["schema"] = schemaFile, ["required"] = check.Required(schemas, kind) },
            };
        }
    }
}
