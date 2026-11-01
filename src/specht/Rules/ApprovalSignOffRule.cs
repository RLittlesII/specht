using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC061 - an approved specification's sign-off agrees with its frontmatter.</summary>
/// <remarks>
/// The rule is guarded on <c>spec_status: approved</c>, which is how a rule
/// that cannot bite yet still ships written rather than deferred: every
/// specification is currently a draft, so it is vacuously green, and the
/// first specification to be approved gets the check for free.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ApprovalSignOffRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC061";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => [Id];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtViolation> Evaluate(SpechtModel model)
    {
        var roles = model.Schemas.Structure.Roles;
        var markers = model.Schemas.Structure.Markers;

        foreach (var feature in model.Features)
        {
            if (!string.Equals(feature.SpecStatus, markers["approved"], StringComparison.Ordinal))
            {
                continue;
            }

            var signOff = feature.Document.Section(roles["signOff"]);

            if (signOff is null)
            {
                continue;
            }

            for (var index = 0; index < signOff.Rows.Count; index++)
            {
                var row = signOff.Rows[index];

                if (row.Any(cell => cell.Contains(markers["draft"], StringComparison.Ordinal) || cell.Contains(markers["blocked"], StringComparison.Ordinal)))
                {
                    yield return new SpechtViolation(
                        Id,
                        SpechtSeverity.Error,
                        feature.RelativePath,
                        signOff.RowLines[index],
                        row.Count > 0 ? row[0].Trim() : null,
                        "spec_status is 'approved' while § 12 carries a row that is not approved")
                    {
                        Expected = new JsonObject { ["section"] = signOff.Title, ["row"] = index + 1 },
                    };
                }
            }
        }
    }
}
