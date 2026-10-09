namespace specht.Report;

/// <summary>
/// The report document (<c>0001-F3</c> B-005 to B-008, B-021): the shape <c>docs/schema/report.schema.json</c> publishes,
/// made from one <see cref="SpecCheckReport"/>. A function of the report alone, so nothing in it reads the clock, the
/// machine or the environment (B-006, C-3).
/// </summary>
/// <param name="SchemaVersion">The schema version checked against.</param>
/// <param name="SchemaSource">Where the schemas the check used came from.</param>
/// <param name="Layouts">Each layout, with its specification count.</param>
/// <param name="ItemCount">How many items sit beside the specifications.</param>
/// <param name="RulesEvaluated">How many rule ids were evaluated.</param>
/// <param name="ErrorCount">How many violations are errors.</param>
/// <param name="WarningCount">How many violations are warnings.</param>
/// <param name="Violations">Every violation, in the report's order.</param>
public sealed record SpecReportDocument(
    int SchemaVersion,
    SpecSchemaSource SchemaSource,
    IReadOnlyList<SpecReportLayout> Layouts,
    int ItemCount,
    int RulesEvaluated,
    int ErrorCount,
    int WarningCount,
    IReadOnlyList<SpecReportViolation> Violations)
{
    /// <summary>Makes the document for <paramref name="report"/>.</summary>
    /// <param name="report">The run's report.</param>
    /// <returns>The document.</returns>
    public static SpecReportDocument From(SpecCheckReport report) =>
        throw new NotImplementedException("0034: the report document is not built yet.");

    /// <summary>Serializes the document as the JSON the published report schema describes.</summary>
    /// <returns>The JSON text.</returns>
    public string ToJson() => throw new NotImplementedException("0034: the report document is not serialized yet.");
}
