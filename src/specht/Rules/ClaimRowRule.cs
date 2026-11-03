using System.Text.Json.Nodes;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC031 - the traceability matrix carries exactly one row per claim.</summary>
/// <remarks>
/// A <c>Missing</c> test cell is deliberately not a violation here. It is the
/// honest value until the test exists, and it blocks the issue reaching done
/// rather than the specification being agreed - see <see cref="ApprovalCoverageRule"/> for the point
/// at which it does become one.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ClaimRowRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC031";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();
        var structure = model.Schemas.Structure;

        foreach (var feature in model.Features)
        {
            var claims = model.Claims[feature];
            var matrix = model.MatrixSections[feature];

            if (claims.Section is null || matrix is null)
            {
                continue;
            }

            foreach (var finding in CheckMatrix(feature, claims.Ids, matrix, structure))
            {
                yield return finding;
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-009" />
    private static IEnumerable<SpechtFinding> CheckMatrix(
        FeatureSpec feature,
        IReadOnlyDictionary<string, int> claims,
        SpechtSection matrix,
        SpechtStructure structure)
    {
        var claimsReference = structure.SectionReference("claims");
        var matrixReference = structure.SectionReference("matrix");
        var withdrawn = structure.Markers.GetValueOrDefault("withdrawn", "Withdrawn");
        var rows = new Dictionary<string, List<int>>(StringComparer.Ordinal);

        for (var index = 0; index < matrix.Rows.Count; index++)
        {
            var id = FirstCell(matrix.Rows[index]);

            if (id.Length == 0)
            {
                continue;
            }

            if (!rows.TryGetValue(id, out var lines))
            {
                lines = [];
                rows[id] = lines;
            }

            lines.Add(matrix.RowLines[index]);
        }

        foreach (var (id, line) in claims)
        {
            if (!rows.ContainsKey(id))
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    line,
                    id,
                    $"claim '{id}' has no row in {matrixReference} - every {claimsReference} claim appears there exactly once, "
                        + $"with '{structure.Markers["missing"]}' as its test until one exists")
                {
                    Expected = MatrixExpected(claims, rows),
                };
            }
            else if (rows[id].Count > 1)
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    rows[id][1],
                    id,
                    $"claim '{id}' has {rows[id].Count} rows in {matrixReference} - expected exactly one")
                {
                    Expected = MatrixExpected(claims, rows),
                };
            }
        }

        foreach (var (id, lines) in rows.Where(row => !claims.ContainsKey(row.Key)))
        {
            yield return new SpechtFinding(
                feature.RelativePath,
                lines[0],
                id,
                $"{matrixReference} cites '{id}', which is not a claim in {claimsReference} - a withdrawn claim is marked {withdrawn}, not deleted")
            {
                Expected = MatrixExpected(claims, rows),
            };
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonObject MatrixExpected(IReadOnlyDictionary<string, int> claims, IReadOnlyDictionary<string, List<int>> rows)
    {
        var counts = new JsonObject();
        foreach (var id in claims.Keys)
        {
            counts[id] = rows.TryGetValue(id, out var lines) ? lines.Count : 0;
        }

        return new JsonObject { ["requiredRows"] = 1, ["rowsByClaim"] = counts };
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static string FirstCell(IReadOnlyList<string> row) => row.Count == 0 ? string.Empty : row[0].Trim();
}
