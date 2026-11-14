using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>SPEC022 checks, under title matching, that every Scenario cell title names a companion scenario carrying the row's claim tag.</summary>
/// <param name="resolver">The shared, pure matrix resolver.</param>
/// <implements epic="0001" feature="F12" claim="B-008" />
public sealed class ScenarioTitleRule(MatrixResolver resolver) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC022";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F12" claim="B-010" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var structure = model.Schemas.Structure;
        if (!structure.MatrixScenarioTitles)
        {
            yield break;
        }

        var matrixReference = structure.SectionReference("matrix");
        foreach (var feature in model.Features.Where(static feature => feature.ClaimFileSet is null))
        {
            foreach (var row in resolver.Resolve(feature, structure, []))
            {
                foreach (var title in row.Scenarios
                    .Where(static title => title.Status is MatrixScenarioStatus.NoSuchTitle or MatrixScenarioStatus.Untagged)
                    .DistinctBy(static title => title.Title, StringComparer.Ordinal))
                {
                    var claim = row.Claim.Trim();
                    yield return new SpechtFinding(
                        feature.RelativePath, row.Line, claim, title.Status == MatrixScenarioStatus.NoSuchTitle
                            ? $"{matrixReference} names scenario '{title.Title}', which no Scenario or Scenario Outline in the companion has"
                            : $"{matrixReference} names scenario '{title.Title}', which does not carry the tag of {claim}");
                }
            }
        }
    }
}
