using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>SPEC035 checks that retired claims have no companion tags and retain their marker in the matrix.</summary>
/// <param name="resolver">The shared, pure claim and matrix resolver.</param>
/// <implements epic="0001" feature="F12" claim="B-001" />
public sealed class RetiredClaimRule(MatrixResolver resolver) : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC035";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => [Id];

    /// <inheritdoc />
    public SpecRuleLevel DefaultLevel => SpecRuleLevel.Off;

    /// <inheritdoc />
    /// <implements epic="0001" feature="F12" claim="B-001" />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var structure = model.Schemas.Structure;
        var markers = structure.Markers;
        foreach (var feature in model.Features)
        {
            foreach (var claim in resolver.ResolveClaims(feature, structure, model.Claims[feature]))
            {
                if (!string.Equals(claim.Status, markers["withdrawn"], StringComparison.Ordinal)
                    && !string.Equals(claim.Status, markers["superseded"], StringComparison.Ordinal))
                {
                    continue;
                }

                if (claim.Tags.Count > 0 || claim.Rows.Any(row => !string.Equals(row.Status.Trim(), claim.Status, StringComparison.Ordinal)))
                {
                    yield return new SpecViolation(
                        Id, SpecSeverity.Error, feature.RelativePath, claim.Claim.Line, claim.Claim.Id,
                        $"retired claim must have no companion tag and its § 9 status must be '{claim.Status}'");
                }
            }
        }
    }
}
