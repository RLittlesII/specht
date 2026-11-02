using System.Text.Json.Nodes;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Rules;

/// <summary>SPEC060 - a done specification, or below schema 0.3.0 an approved one, has no missing coverage.</summary>
/// <remarks>
/// From schema 0.3.0 the rule selects a specification whose delivery <c>status</c> is the manifest's done marker,
/// whatever its <c>spec_status</c>; below it, one whose <c>spec_status</c> is the approved marker
/// (<c>0001-F7</c> decision 0015).
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
/// <implements epic="0001" feature="F7" claim="B-118" />
public sealed class ApprovalCoverageRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC060";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F7" claim="B-118" />
    /// <implements epic="0001" feature="F7" claim="B-119" />
    /// <implements epic="0001" feature="F7" claim="B-120" />
    /// <implements epic="0001" feature="F7" claim="B-121" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var markers = model.Schemas.Structure.Markers;
        var doneAxis = model.Schemas.Structure.SchemaVersion.CompareTo(DoneAxisVersion) >= 0;
        var message = doneAxis
            ? "status is 'done' while § 9 still carries a 'Missing' cell"
            : "spec_status is 'approved' while § 9 still carries a 'Missing' cell";

        foreach (var feature in model.Features)
        {
            var selected = doneAxis
                ? string.Equals(feature.Status, markers["done"], StringComparison.Ordinal)
                : string.Equals(feature.SpecStatus, markers["approved"], StringComparison.Ordinal);

            if (!selected)
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
                        message)
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

    private static readonly SemanticVersion DoneAxisVersion = new(0, 3, 0);
}
