using Specht.Model;

namespace Specht.Rules;

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
        model.EnsureClaimGrammar();
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

            var claims = model.Claims[feature].TagIds;
            var companion = feature.FeatureFiles[0];

            foreach (var tag in companion.Tags)
            {
                if (!claims.Contains(tag.Id))
                {
                    yield return new SpecViolation(
                        "SPEC021",
                        SpecSeverity.Error,
                        companion.RelativePath,
                        tag.Line,
                        tag.Id,
                        $"scenario tag '{tag.Text ?? "@" + tag.Id}' does not resolve to a claim in § 3 of {feature.RelativePath}");
                }
            }
        }
    }

}
