using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC030 - claim ids are well-formed and unique within their Feature.</summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ClaimIdRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC030";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    /// <implements epic="0001" feature="F5" claim="B-076" />
    /// <implements epic="0001" feature="F5" claim="B-087" />
    /// <implements epic="0001" feature="F5" claim="B-100" />
    /// <implements epic="0001" feature="F5" claim="B-101" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();
        var structure = model.Schemas.Structure;
        var manifestGrammar = structure.ClaimGrammar(null);
        var claimsReference = structure.SectionReference("claims");

        foreach (var feature in model.Features)
        {
            var claims = model.Claims[feature];
            var claimGrammar = feature.ClaimGrammar ?? manifestGrammar;
            var (grammar, prefix) = claimGrammar;
            var form = prefix is null ? grammar
                : claimGrammar.Head is { } head ? $"the claim prefix '{head}' followed by a whole match of ids.claim.id"
                : $"the claim prefix '{prefix}' followed by three digits and an optional lowercase letter";

            if (claimGrammar.SharedHead is { } shared)
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(structure.FeatureKeys.ClaimPrefix),
                    shared,
                    $"{structure.FeatureKeys.ClaimPrefix} gives claims and scenarios the one head '{shared}', so the Feature is read as declaring no prefix")
                {
                    Expected = new JsonObject { ["grammar"] = grammar },
                };
            }

            if (claims.Section is null)
            {
                continue;
            }

            var scenarioRest = claimGrammar.LongerScenarioHead
                && claimGrammar.ScenarioHead!.StartsWith(claimGrammar.Head!, structure.TagComparison)
                    ? claimGrammar.ScenarioHead[claimGrammar.Head!.Length..]
                    : null;

            foreach (var claim in claims.Rows)
            {
                var id = claim.Id;
                var line = claim.Line;

                if (id.Length == 0)
                {
                    continue;
                }

                if (!claim.IsValid)
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        line,
                        id,
                        $"claim id '{id}' does not match {form} - claim ids are permanent, so the form is fixed")
                    {
                        Expected = new JsonObject { ["grammar"] = grammar },
                    };

                    continue;
                }

                if (scenarioRest is not null && id[claimGrammar.Head!.Length..].StartsWith(scenarioRest, structure.TagComparison))
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        line,
                        id,
                        $"claim id '{id}' begins with the scenario head '{claimGrammar.ScenarioHead}', so its tag would read as a scenario tag")
                    {
                        Expected = new JsonObject { ["grammar"] = grammar },
                    };

                    continue;
                }

                if (claim.FirstDeclarationLine is { } firstDeclarationLine)
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        line,
                        id,
                        $"claim '{id}' is declared twice in {claimsReference} (also at line {firstDeclarationLine})")
                    {
                        Expected = new JsonObject { ["grammar"] = grammar, ["firstDeclarationLine"] = firstDeclarationLine },
                    };
                }
            }
        }
    }
}
