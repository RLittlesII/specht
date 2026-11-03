using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// SPEC021 - every claim tag in a specification's one companion Gherkin file
/// resolves to a § 3 claim.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ClaimTagRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC021";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();
        var claimsReference = model.Schemas.Structure.SectionReference("claims");
        foreach (var feature in model.Features)
        {
            if (feature.FeatureFiles.Count != 1)
            {
                continue;
            }

            var claims = model.Claims[feature].TagIds;
            var companion = feature.FeatureFiles[0];

            foreach (var tag in companion.Tags)
            {
                if (!claims.Contains(tag.Id))
                {
                    yield return new SpechtFinding(
                        companion.RelativePath,
                        tag.Line,
                        tag.Id,
                        $"scenario tag '{tag.Text ?? "@" + tag.Id}' does not resolve to a claim in {claimsReference} of {feature.RelativePath}")
                    {
                        Expected = new JsonObject
                        {
                            ["claims"] = new JsonArray(model.Claims[feature].Ids.Keys.Select(static id => (JsonNode?)JsonValue.Create(id)).ToArray()),
                        },
                    };
                }
            }
        }
    }
}
