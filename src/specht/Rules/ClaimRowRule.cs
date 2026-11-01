using System.Text.Json.Nodes;
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
    public IReadOnlyList<string> ReportedIds => [Id];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtViolation> Evaluate(SpechtModel model)
    {
        model.EnsureClaimGrammar();

        foreach (var feature in model.Features)
        {
            var claims = model.Claims[feature];
            var matrix = model.MatrixSections[feature];

            if (claims.Section is null || matrix is null)
            {
                continue;
            }

            foreach (var violation in CheckMatrix(feature, claims.Ids, matrix))
            {
                yield return violation;
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static IEnumerable<SpechtViolation> CheckMatrix(
        FeatureSpec feature,
        IReadOnlyDictionary<string, int> claims,
        SpechtSection matrix)
    {
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
                yield return new SpechtViolation(
                    "SPEC031",
                    SpechtSeverity.Error,
                    feature.RelativePath,
                    line,
                    id,
                    $"claim '{id}' has no row in § 9 - every § 3 claim appears there exactly once, "
                        + "with 'Missing' as its test until one exists")
                {
                    Expected = MatrixExpected(claims, rows),
                };
            }
            else if (rows[id].Count > 1)
            {
                yield return new SpechtViolation(
                    "SPEC031",
                    SpechtSeverity.Error,
                    feature.RelativePath,
                    rows[id][1],
                    id,
                    $"claim '{id}' has {rows[id].Count} rows in § 9 - expected exactly one")
                {
                    Expected = MatrixExpected(claims, rows),
                };
            }
        }

        foreach (var (id, lines) in rows.Where(row => !claims.ContainsKey(row.Key)))
        {
            yield return new SpechtViolation(
                "SPEC031",
                SpechtSeverity.Error,
                feature.RelativePath,
                lines[0],
                id,
                $"§ 9 cites '{id}', which is not a claim in § 3 - a withdrawn claim is marked Withdrawn, not deleted")
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
