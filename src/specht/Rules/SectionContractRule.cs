using System.Text.Json.Nodes;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC010 - a specification carries the contracted sections, exactly once each and in order.</summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SectionContractRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC010";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var expected = model.Schemas.Structure.Sections;

        foreach (var feature in model.Features.Where(static feature => feature.ClaimFileSet is null))
        {
            var titles = feature.Document.Sections.Select(section => section.Title).ToList();

            foreach (var title in expected)
            {
                var count = titles.Count(candidate => string.Equals(candidate, title, StringComparison.Ordinal));

                if (count == 0)
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        0,
                        title,
                        $"missing section '## {title}' - the contracted sections are in {SpechtManifestStore.RelativePath}")
                    { Expected = Sections(expected) };
                }
                else if (count > 1)
                {
                    yield return new SpechtFinding(
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

                    yield return new SpechtFinding(
                        feature.RelativePath,
                        section?.Line ?? 0,
                        ordered[index],
                        $"section '## {ordered[index]}' is out of order - '## {present[index]}' is expected at this position")
                    { Expected = Sections(expected) };

                    break;
                }
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonObject Sections(IEnumerable<string> titles) =>
        new() { ["sections"] = new JsonArray([.. titles.Select(static title => JsonValue.Create(title))]) };
}
