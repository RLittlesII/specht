namespace specht.Rules;

/// <summary>
/// SPEC060 and SPEC061 - an approved specification has no missing coverage and
/// its sign-off agrees with its frontmatter.
/// </summary>
/// <remarks>
/// Both rules are guarded on <c>spec_status: approved</c>, which is how a rule
/// that cannot bite yet still ships written rather than deferred: every
/// specification is currently a draft, so they are vacuously green, and the
/// first specification to be approved gets the check for free.
/// </remarks>
public sealed class ApprovalRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC060";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC060", "SPEC061"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        foreach (var feature in model.Features)
        {
            if (!string.Equals(feature.SpecStatus, Approved, StringComparison.Ordinal))
            {
                continue;
            }

            var matrix = feature.Document.Section(TraceabilityMatrix);

            if (matrix is not null)
            {
                for (var index = 0; index < matrix.Rows.Count; index++)
                {
                    var row = matrix.Rows[index];

                    if (row.Any(static cell => string.Equals(cell.Trim(), Missing, StringComparison.Ordinal)))
                    {
                        yield return new SpecViolation(
                            "SPEC060",
                            SpecSeverity.Error,
                            feature.RelativePath,
                            matrix.RowLines[index],
                            row.Count > 0 ? row[0].Trim() : null,
                            "spec_status is 'approved' while § 9 still carries a 'Missing' cell");
                    }
                }
            }

            var signOff = feature.Document.Section(SignOff);

            if (signOff is null)
            {
                continue;
            }

            for (var index = 0; index < signOff.Rows.Count; index++)
            {
                var row = signOff.Rows[index];

                if (row.Any(NotApproved))
                {
                    yield return new SpecViolation(
                        "SPEC061",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        signOff.RowLines[index],
                        row.Count > 0 ? row[0].Trim() : null,
                        "spec_status is 'approved' while § 12 carries a row that is not approved");
                }
            }
        }
    }

    private static bool NotApproved(string cell) =>
        cell.Contains(Draft, StringComparison.Ordinal) || cell.Contains(Blocked, StringComparison.Ordinal);

    private const string TraceabilityMatrix = "9. Traceability Matrix";
    private const string SignOff = "12. Sign-off";
    private const string Approved = "approved";
    private const string Missing = "Missing";
    private const string Draft = "\U0001F7E1";
    private const string Blocked = "\U0001F534";
}
