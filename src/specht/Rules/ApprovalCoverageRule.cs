using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC060 - an approved specification has no missing coverage.</summary>
/// <remarks>
/// The rule is guarded on <c>spec_status: approved</c>, which is how a rule
/// that cannot bite yet still ships written rather than deferred: every
/// specification is currently a draft, so it is vacuously green, and the
/// first specification to be approved gets the check for free.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class ApprovalCoverageRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC060";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var markers = model.Schemas.Structure.Markers;

        foreach (var feature in model.Features)
        {
            if (!string.Equals(feature.SpecStatus, markers["approved"], StringComparison.Ordinal))
            {
                continue;
            }

            var matrix = model.MatrixSections[feature];

            if (matrix is null)
            {
                continue;
            }

            for (var index = 0; index < matrix.Rows.Count; index++)
            {
                var row = matrix.Rows[index];

                if (row.Any(cell => string.Equals(cell.Trim(), markers["missing"], StringComparison.Ordinal)))
                {
                    yield return new SpechtFinding(
                        feature.RelativePath,
                        matrix.RowLines[index],
                        row.Count > 0 ? row[0].Trim() : null,
                        "spec_status is 'approved' while § 9 still carries a 'Missing' cell")
                    {
                        Expected = new JsonObject
                        {
                            ["section"] = matrix.Title,
                            ["row"] = index + 1,
                            ["columns"] = new JsonArray(row.Select(static (cell, column) => (Cell: cell, Column: column + 1))
                                .Where(cell => string.Equals(cell.Cell.Trim(), markers["missing"], StringComparison.Ordinal))
                                .Select(static cell => (JsonNode?)JsonValue.Create(cell.Column)).ToArray()),
                        },
                    };
                }
            }
        }
    }
}
