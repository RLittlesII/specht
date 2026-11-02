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
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();
        var grammar = model.Schemas.Structure.Identifiers["claim"];

        foreach (var feature in model.Features)
        {
            var claims = model.Claims[feature];

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
                        $"claim id '{id}' does not match {grammar} - claim ids are permanent, so the form is fixed")
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
                        $"claim '{id}' is declared twice in § 3 (also at line {firstDeclarationLine})")
                    {
                        Expected = new JsonObject { ["grammar"] = grammar, ["firstDeclarationLine"] = firstDeclarationLine },
                    };
                }
            }
        }
    }
}
