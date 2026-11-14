using Specht.Evaluation;
using Specht.Model;

namespace Specht.Report;

/// <summary>
/// The report document (<c>0001-F3</c> B-005 to B-008, B-021): the shape <c>docs/schema/report.schema.json</c> publishes,
/// made from one <see cref="SpechtReport"/>. A function of the report alone, so nothing in it reads the clock, the
/// machine or the environment (B-006, C-3).
/// </summary>
/// <param name="SchemaVersion">The schema version checked against, as <c>major.minor.patch</c>.</param>
/// <param name="SchemaSource">Where the schemas the check used came from.</param>
/// <param name="Layouts">Each layout, with its specification count.</param>
/// <param name="ItemCount">How many items sit beside the specifications.</param>
/// <param name="RulesEvaluated">How many rule ids were evaluated.</param>
/// <param name="ErrorCount">How many violations are errors.</param>
/// <param name="WarningCount">How many violations are warnings.</param>
/// <param name="Violations">Every violation, in the report's order.</param>
/// <implements epic="0001" feature="F3" claim="B-001" />
public sealed record SpechtReportDocument(
    string SchemaVersion,
    SpechtSchemaSource SchemaSource,
    IReadOnlyList<SpechtReportLayout> Layouts,
    int ItemCount,
    int RulesEvaluated,
    int ErrorCount,
    int WarningCount,
    IReadOnlyList<SpechtReportViolation> Violations)
{
    /// <summary>Makes the run summary the check prints after the violation lines (<c>0001-F2</c> B-002).</summary>
    /// <returns>The summary lines.</returns>
    /// <implements epic="0001" feature="F3" claim="B-001" />
    public IReadOnlyList<string> SummaryLines() =>
    [
        $"specifications: {string.Join(", ", Layouts.Select(Count))}",
        FormattableString.Invariant($"items: {ItemCount}"),
        FormattableString.Invariant($"rules evaluated: {RulesEvaluated}"),
        FormattableString.Invariant($"errors: {ErrorCount}, warnings: {WarningCount}"),
    ];

    /// <implements epic="0001" feature="F3" claim="B-001" />
    private static string Count(SpechtReportLayout layout) =>
        FormattableString.Invariant($"{layout.Layout} {layout.SpecificationCount}");
}
