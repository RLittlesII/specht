using Specht.Model;

namespace Specht.Rules.Matrix;

/// <summary>SPEC033 reports, at its matrix row, a Status the row's scenario and tests do not bear out, or one that is no marker.</summary>
/// <param name="resolver">The shared, pure claim and matrix resolver.</param>
/// <implements epic="0001" feature="F12" claim="B-013" />
public sealed class StatusRule(MatrixResolver resolver) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC033";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F12" claim="B-012" />
    /// <implements epic="0001" feature="F12" claim="B-013" />
    /// <implements epic="0001" feature="F12" claim="B-014" />
    /// <implements epic="0001" feature="F12" claim="B-015" />
    /// <implements epic="0001" feature="F12" claim="B-016" />
    /// <implements epic="0001" feature="F12" claim="B-017" />
    /// <implements epic="0001" feature="F12" claim="B-028" />
    /// <implements epic="0001" feature="F12" claim="B-029" />
    /// <implements epic="0001" feature="F12" claim="B-030" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var structure = model.Schemas.Structure;
        var markers = structure.Markers;
        var matrixReference = structure.SectionReference("matrix");
        var configured = structure.Tests is not null;
        string[] known = ["covered", "partial", "missing", "waived", "withdrawn", "superseded"];
        foreach (var feature in model.Features.Where(static feature => feature.ClaimFileSet is null))
        {
            var claims = resolver.ResolveClaims(feature, structure, model.Claims[feature])
                .ToLookup(static claim => claim.Claim.Id, StringComparer.Ordinal);
            var tags = resolver.ClaimTags(feature, structure);
            foreach (var row in resolver.Resolve(feature, structure, model.TestFiles))
            {
                var id = row.Claim.Trim();
                var status = row.Status.Trim();
                if (claims[id].Any(claim => claim.MissesScenarioAt(row, markers)))
                {
                    continue;
                }

                var scenario = tags[id].Any()
                    && (!structure.MatrixScenarioTitles
                        || row.Scenarios.Count > 0 && row.Scenarios.All(static title => title.Status == MatrixScenarioStatus.Resolved));
                var named = row.Tests.Count > 0;
                var resolved = row.Tests.Count(static test => test.Status == MatrixTestStatus.Resolved);
                var marker = known.FirstOrDefault(key => string.Equals(status, markers[key], StringComparison.Ordinal));
                var message = marker switch
                {
                    "covered" => Covered(scenario, named, !configured || resolved == row.Tests.Count) is { } reason
                        ? $"{matrixReference} status '{status}' overclaims: {reason}"
                        : null,
                    "partial" when !scenario && resolved == 0 =>
                        $"{matrixReference} status '{status}' overclaims: neither its scenario nor any test it names resolves",
                    "missing" when configured && named && resolved == row.Tests.Count =>
                        $"{matrixReference} status '{status}' is stale: every test it names resolves",
                    null => $"{matrixReference} status '{status}' is not a marker; expected "
                        + string.Join(", ", known.Select(key => $"'{markers[key]}'")),
                    _ => null,
                };
                if (message is not null)
                {
                    yield return new SpechtFinding(feature.RelativePath, row.Line, id, message);
                }
            }
        }
    }

    /// <implements epic="0001" feature="F12" claim="B-013" />
    private static string? Covered(bool scenario, bool named, bool resolved) =>
        !scenario ? "its scenario does not resolve"
        : !named ? "its Test cell names no test in inline code"
        : !resolved ? "a test it names does not resolve"
        : null;
}
