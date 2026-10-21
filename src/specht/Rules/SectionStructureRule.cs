using System.Text.Json.Nodes;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC010 and SPEC013 - a specification carries the contracted sections,
/// exactly once each and in order, and the tables that gate coverage have the
/// headers the contract names.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SectionStructureRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC010";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC010", "SPEC013"];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var expected = model.Schemas.Structure.Sections;

        foreach (var feature in model.Features)
        {
            var titles = feature.Document.Sections.Select(section => section.Title).ToList();

            foreach (var title in expected)
            {
                var count = titles.Count(candidate => string.Equals(candidate, title, StringComparison.Ordinal));

                if (count == 0)
                {
                    yield return new SpecViolation(
                        "SPEC010",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        0,
                        title,
                        $"missing section '## {title}' - the contracted sections are in {SpecManifestStore.RelativePath}")
                    { Expected = Sections(expected) };
                }
                else if (count > 1)
                {
                    yield return new SpecViolation(
                        "SPEC010",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        0,
                        title,
                        $"section '## {title}' appears {count} times - each contracted section appears exactly once")
                    { Expected = Sections(expected) };
                }
            }

            var present = expected.Where(title => titles.Contains(title, StringComparer.Ordinal)).ToList();
            var ordered = titles.Where(title => expected.Contains(title, StringComparer.Ordinal)).Distinct().ToList();

            for (var index = 0; index < Math.Min(present.Count, ordered.Count); index++)
            {
                if (!string.Equals(present[index], ordered[index], StringComparison.Ordinal))
                {
                    var section = feature.Document.Section(ordered[index]);

                    yield return new SpecViolation(
                        "SPEC010",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        section?.Line ?? 0,
                        ordered[index],
                        $"section '## {ordered[index]}' is out of order - '## {present[index]}' is expected at this position")
                    { Expected = Sections(expected) };

                    break;
                }
            }

            foreach (var violation in CheckTables(model, feature))
            {
                yield return violation;
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static IEnumerable<SpecViolation> CheckTables(SpecModel model, FeatureSpec feature)
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
                yield return new SpecViolation(
                    "SPEC013",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    section.Line,
                    title,
                    $"section '## {title}' carries no table - expected a table with headers {string.Join(" | ", headers)}")
                { Expected = Table(title, headers) };

                continue;
            }

            if (!section.Headers.SequenceEqual(headers, StringComparer.Ordinal))
            {
                yield return new SpecViolation(
                    "SPEC013",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    section.Line,
                    title,
                    $"section '## {title}' has headers {string.Join(" | ", section.Headers)} - expected {string.Join(" | ", headers)}")
                { Expected = Table(title, headers) };
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonObject Sections(IEnumerable<string> titles) => new() { ["sections"] = Strings(titles) };

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonObject Table(string title, IEnumerable<string> headers) => new() { ["section"] = title, ["headers"] = Strings(headers) };

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonArray Strings(IEnumerable<string> values) => new([.. values.Select(static value => JsonValue.Create(value))]);
}
