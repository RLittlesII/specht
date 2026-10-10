using System.Text.RegularExpressions;

namespace specht.Rules;

/// <summary>
/// SPEC020 and SPEC021 - a specification has exactly one companion Gherkin
/// file beside it, and every claim tag in that file resolves to a § 3 claim.
/// </summary>
public sealed class FeatureFileRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC020";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC020", "SPEC021"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var grammar = new Regex(model.Schemas.Structure.Identifiers["claim"], RegexOptions.Compiled);

        foreach (var feature in model.Features)
        {
            if (feature.FeatureFiles.Count != 1)
            {
                yield return new SpecViolation(
                    "SPEC020",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    0,
                    feature.Identity,
                    $"found {feature.FeatureFiles.Count} '.feature' files beside this specification - expected exactly one companion");

                continue;
            }

            var claims = ClaimIds(model, feature, grammar);
            var path = feature.FeatureFiles[0];
            var relative = SpecDiscovery.Relative(model.Root, path);

            foreach (var tag in FeatureFileReader.ReadTags(path))
            {
                if (!claims.Contains(tag.Id))
                {
                    yield return new SpecViolation(
                        "SPEC021",
                        SpecSeverity.Error,
                        relative,
                        tag.Line,
                        tag.Id,
                        $"scenario tag '@{tag.Id}' does not resolve to a claim in § 3 of {feature.RelativePath}");
                }
            }
        }
    }

    private static HashSet<string> ClaimIds(SpecModel model, FeatureSpec feature, Regex grammar)
    {
        var section = feature.Document.Section(model.Schemas.Structure.Roles["claims"]);
        var ids = new HashSet<string>(StringComparer.Ordinal);

        if (section is null)
        {
            return ids;
        }

        foreach (var row in section.Rows.Where(static row => row.Count > 0))
        {
            var id = row[0].Trim();

            if (grammar.IsMatch(id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }
}
