using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>SPEC023 reports, at its matrix row, a live claim whose tag no companion scenario carries, unless the row is missing or waived.</summary>
/// <param name="resolver">The shared, pure claim and matrix resolver.</param>
/// <implements epic="0001" feature="F12" claim="B-011" />
public sealed class MissingScenarioRule(MatrixResolver resolver) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC023";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F12" claim="B-011" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var structure = model.Schemas.Structure;
        var markers = structure.Markers;
        var matrixReference = structure.SectionReference("matrix");
        foreach (var feature in model.Features)
        {
            foreach (var claim in resolver.ResolveClaims(feature, structure, model.Claims[feature]))
            {
                foreach (var row in claim.Rows.Where(row => claim.MissesScenarioAt(row, markers)))
                {
                    yield return new SpechtFinding(
                        feature.RelativePath, row.Line, claim.Claim.Id,
                        $"no companion scenario carries the tag of {claim.Claim.Id}, and its {matrixReference} status is not '{markers["missing"]}'");
                }
            }
        }
    }
}
