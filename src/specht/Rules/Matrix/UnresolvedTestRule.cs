using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>SPEC032 checks that every matrix test name resolves in a declared test file and that a Test cell names one in inline code.</summary>
/// <param name="resolver">The shared, pure matrix resolver.</param>
/// <implements epic="0001" feature="F12" claim="B-002" />
public sealed class UnresolvedTestRule(MatrixResolver resolver) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC032";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F12" claim="B-002" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var structure = model.Schemas.Structure;
        if (structure.Tests is null)
        {
            yield break;
        }

        var markers = structure.Markers;
        var matrixReference = structure.SectionReference("matrix");
        string[] exempt = [markers["waived"], markers["withdrawn"], markers["superseded"]];
        foreach (var feature in model.Features.Where(static feature => feature.ClaimFileSet is null))
        {
            foreach (var row in resolver.Resolve(feature, structure, model.TestFiles))
            {
                if (exempt.Contains(row.Status.Trim(), StringComparer.Ordinal)
                    || string.Equals(row.Test.Trim(), markers["missing"], StringComparison.Ordinal))
                {
                    continue;
                }

                if (row.Tests.Count == 0)
                {
                    yield return new SpechtFinding(
                        feature.RelativePath, row.Line, row.Claim.Trim(), $"{matrixReference} Test cell names no test in inline code");
                    continue;
                }

                foreach (var name in row.Tests
                    .Where(static test => test.Status == MatrixTestStatus.Unresolved)
                    .Select(static test => test.Name)
                    .Distinct(StringComparer.Ordinal))
                {
                    yield return new SpechtFinding(
                        feature.RelativePath, row.Line, row.Claim.Trim(), $"{matrixReference} names test '{name}', which no declared test file holds");
                }
            }
        }
    }
}
