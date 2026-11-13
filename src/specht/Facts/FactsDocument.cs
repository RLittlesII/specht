using System.Text.Json;

using Specht.Model;

namespace Specht.Facts;

/// <summary>
/// The facts document (<c>0132-F2</c>): the shape <c>docs/schema/facts.schema.json</c> publishes, projected from one
/// <see cref="SpechtModel"/>. No rule runs, so it carries no violation, count or verdict (B-014), and it is a function of
/// the model alone, so nothing in it reads the clock, the machine or the environment (B-022).
/// </summary>
/// <param name="Specifications">Every discovered Feature specification, in discovery order.</param>
/// <implements epic="0132" feature="F2" claim="B-001" />
/// <implements epic="0132" feature="F2" claim="B-014" />
public sealed record FactsDocument(IReadOnlyList<FeatureFacts> Specifications)
{
    /// <summary>
    /// Makes the document for <paramref name="model"/>: every claim and constraint row is given, none judged (B-013), and
    /// its epic files, traceability matrices and test files are not read (B-025, B-026).
    /// </summary>
    /// <param name="model">The model the check's own read gives.</param>
    /// <returns>The document.</returns>
    /// <implements epic="0132" feature="F2" claim="B-001" />
    /// <implements epic="0132" feature="F2" claim="B-003" />
    /// <implements epic="0132" feature="F2" claim="B-004" />
    /// <implements epic="0132" feature="F2" claim="B-005" />
    /// <implements epic="0132" feature="F2" claim="B-006" />
    /// <implements epic="0132" feature="F2" claim="B-012" />
    /// <implements epic="0132" feature="F2" claim="B-013" />
    /// <implements epic="0132" feature="F2" claim="B-017" />
    /// <implements epic="0132" feature="F2" claim="B-025" />
    /// <implements epic="0132" feature="F2" claim="B-026" />
    public static FactsDocument From(SpechtModel model)
    {
        var identified = model.FeaturesWithIdentity.ToHashSet();
        var constraintsListed = model.Schemas.Structure.Sections.Contains(ConstraintsTitle, StringComparer.Ordinal);
        return new(model.Features
            .Select(feature => new FeatureFacts(
                feature.RelativePath,
                identified.Contains(feature) ? feature.Identity : null,
                feature.SpecStatus,
                feature.Status,
                feature.Strings(feature.Keys.DependsOn),
                feature.Strings(feature.Keys.Blocks),
                feature.Strings(feature.Keys.Children),
                Claims(model.Claims[feature]),
                Constraints(constraintsListed ? feature.Document.Section(ConstraintsTitle) : null)))
            .ToList());
    }

    /// <summary>Serializes the document as the JSON the published facts schema describes, every member written.</summary>
    /// <returns>The JSON text.</returns>
    /// <implements epic="0132" feature="F2" claim="B-002" />
    /// <implements epic="0132" feature="F2" claim="B-022" />
    public string ToJson() => JsonSerializer.Serialize(this, Options);

    /// <implements epic="0132" feature="F2" claim="B-005" />
    /// <implements epic="0132" feature="F2" claim="B-012" />
    /// <implements epic="0132" feature="F2" claim="B-013" />
    private static List<ClaimFacts> Claims(SpechtClaims claims) =>
        claims.Section is not { } section
            ? []
            : claims.Rows
                .Select((claim, row) => new ClaimFacts(claim.Id, Cell(section, row, "Claim"), Cell(section, row, "Status"), claim.Line))
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-006" />
    private static List<ConstraintFacts> Constraints(SpechtSection? section) =>
        section is null
            ? []
            : section.Rows
                .Select((cells, row) => new ConstraintFacts(
                    cells.Count == 0 ? string.Empty : cells[0],
                    Cell(section, row, "Constraint"),
                    Cell(section, row, "Rules Out"),
                    section.RowLines[row]))
                .ToList();

    /// <implements epic="0132" feature="F2" claim="B-005" />
    /// <implements epic="0132" feature="F2" claim="B-006" />
    private static string? Cell(SpechtSection section, int row, string header)
    {
        var index = section.Headers.ToList().IndexOf(header);
        var cells = section.Rows[row];
        return index >= 0 && index < cells.Count ? cells[index] : null;
    }

    private const string ConstraintsTitle = "4. Constraints";

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        NewLine = "\n",
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
