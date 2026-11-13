using System.Text.Json.Nodes;
using Specht.Evaluation;

namespace Specht.Report;

/// <summary>Makes the report document from one report (<c>0001-F3</c> B-005 to B-007; ADR-0010 § "The layout"; item <c>0167</c>).</summary>
/// <implements epic="0001" feature="F3" claim="B-001" />
public sealed class SpechtReportDocumentMapper
{
    /// <summary>Makes the document for <paramref name="report"/>.</summary>
    /// <param name="report">The run's report.</param>
    /// <returns>The document.</returns>
    /// <implements epic="0001" feature="F3" claim="B-001" />
    public SpechtReportDocument Map(SpechtReport report) =>
        new(
            report.SchemaVersion.ToString(),
            report.SchemaSource,
            report.Layouts,
            report.ItemCount,
            report.RulesEvaluated,
            report.ErrorCount,
            report.WarningCount,
            report.Violations
                .Select(static violation => new SpechtReportViolation(
                    violation.RuleId,
                    violation.Severity,
                    violation.File,
                    violation.Line,
                    violation.Identifier,
                    violation.Message,
                    violation.Expected?.DeepClone().AsObject() ?? new JsonObject()))
                .ToList());
}
