using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC020 - a specification has exactly one companion Gherkin file beside it.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class CompanionFileRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC020";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();
        foreach (var feature in model.Features)
        {
            if (feature.FeatureFiles.Count != 1)
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    0,
                    feature.Identity,
                    $"found {feature.FeatureFiles.Count} '.feature' files beside this specification - expected exactly one companion")
                {
                    Expected = new JsonObject { ["companionCount"] = 1 },
                };
            }
        }
    }
}
