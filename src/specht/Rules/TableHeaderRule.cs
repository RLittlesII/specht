using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC013 - the tables that gate coverage have the headers the contract names.</summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class TableHeaderRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC013";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var feature in model.Features)
        {
            foreach (var (role, headers) in model.Schemas.Structure.Tables)
            {
                var title = model.Schemas.Structure.Roles[role];
                var section = role switch
                {
                    "claims" => model.Claims[feature].Section,
                    "matrix" => model.MatrixSections[feature],
                    _ => feature.Document.Section(title),
                };

                if (section is null)
                {
                    continue;
                }

                if (section.Headers.Count == 0)
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        section.Line,
                        title,
                        $"section '## {title}' carries no table - expected a table with headers {string.Join(" | ", headers)}")
                    { Expected = Table(title, headers) };

                    continue;
                }

                if (!section.Headers.SequenceEqual(headers, StringComparer.Ordinal))
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        section.Line,
                        title,
                        $"section '## {title}' has headers {string.Join(" | ", section.Headers)} - expected {string.Join(" | ", headers)}")
                    { Expected = Table(title, headers) };
                }
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonObject Table(string title, IEnumerable<string> headers) =>
        new() { ["section"] = title, ["headers"] = new JsonArray([.. headers.Select(static header => JsonValue.Create(header))]) };
}
