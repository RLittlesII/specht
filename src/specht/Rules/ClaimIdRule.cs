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
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();
        var manifestGrammar = model.Schemas.Structure.ClaimGrammar(null);
        var claimsReference = model.Schemas.Structure.SectionReference("claims");

        foreach (var feature in model.Features)
        {
            var claims = model.Claims[feature];
            var (grammar, prefix) = feature.ClaimGrammar ?? manifestGrammar;
            var form = prefix is null ? grammar : $"the claim prefix '{prefix}' followed by three digits and an optional lowercase letter";

            if (claims.Section is null)
            {
                continue;
            }

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
