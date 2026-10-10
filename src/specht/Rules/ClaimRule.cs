using System.Text.RegularExpressions;

namespace specht.Rules;

/// <summary>
/// SPEC030 and SPEC031 - claim ids are well formed and unique within their
/// Feature, and the traceability matrix carries exactly one row per claim.
/// </summary>
/// <remarks>
/// A <c>Missing</c> test cell is deliberately not a violation here. It is the
/// honest value until the test exists, and it blocks the issue reaching done
/// rather than the specification being agreed - see ApprovalRule for the point
/// at which it does become one.
/// </remarks>
public sealed class ClaimRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC030";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC030", "SPEC031"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var grammar = new Regex(model.Schemas.Structure.Identifiers["claim"], RegexOptions.Compiled);

        foreach (var feature in model.Features)
        {
            var claims = feature.Document.Section(model.Schemas.Structure.Roles["claims"]);
            var matrix = feature.Document.Section(model.Schemas.Structure.Roles["matrix"]);

            if (claims is null)
            {
                continue;
            }

            var seen = new Dictionary<string, int>(StringComparer.Ordinal);

            for (var index = 0; index < claims.Rows.Count; index++)
            {
                var id = FirstCell(claims.Rows[index]);
                var line = claims.RowLines[index];

                if (id.Length == 0)
                {
                    continue;
                }

                if (!grammar.IsMatch(id))
                {
                    yield return new SpecViolation(
                        "SPEC030",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        line,
                        id,
                        $"claim id '{id}' does not match {grammar} - claim ids are permanent, so the form is fixed");

                    continue;
                }

                if (!seen.TryAdd(id, line))
                {
                    yield return new SpecViolation(
                        "SPEC030",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        line,
                        id,
                        $"claim '{id}' is declared twice in § 3 (also at line {seen[id]})");
                }
            }

            if (matrix is null)
            {
                continue;
            }

            foreach (var violation in CheckMatrix(feature, seen, matrix))
            {
                yield return violation;
            }
        }
    }

    private static IEnumerable<SpecViolation> CheckMatrix(
        FeatureSpec feature,
        IReadOnlyDictionary<string, int> claims,
        SpecSection matrix)
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
                yield return new SpecViolation(
                    "SPEC031",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    line,
                    id,
                    $"claim '{id}' has no row in § 9 - every § 3 claim appears there exactly once, "
                        + "with 'Missing' as its test until one exists");
            }
            else if (rows[id].Count > 1)
            {
                yield return new SpecViolation(
                    "SPEC031",
                    SpecSeverity.Error,
                    feature.RelativePath,
                    rows[id][1],
                    id,
                    $"claim '{id}' has {rows[id].Count} rows in § 9 - expected exactly one");
            }
        }

        foreach (var (id, lines) in rows.Where(row => !claims.ContainsKey(row.Key)))
        {
            yield return new SpecViolation(
                "SPEC031",
                SpecSeverity.Error,
                feature.RelativePath,
                lines[0],
                id,
                $"§ 9 cites '{id}', which is not a claim in § 3 - a withdrawn claim is marked Withdrawn, not deleted");
        }
    }

    private static string FirstCell(IReadOnlyList<string> row) => row.Count == 0 ? string.Empty : row[0].Trim();
}
