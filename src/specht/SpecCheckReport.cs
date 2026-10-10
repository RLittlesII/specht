using specht.Report;

namespace specht;

/// <summary>The outcome of one <c>SpecCheck</c> run.</summary>
/// <param name="SchemaVersion">The schema version the check ran under (<c>0001-F7</c> B-002).</param>
/// <param name="SpecificationCount">How many specifications were discovered.</param>
/// <param name="Layouts">Each manifest layout, in the manifest's order, with its specification count (<c>0001-F6</c> B-009, C-7).</param>
/// <param name="ItemCount">How many task, test, bug and spike files sit beside them.</param>
/// <param name="RulesEvaluated">How many <c>SPEC###</c> ids were evaluated.</param>
/// <param name="Violations">Every violation, most severe first.</param>
public sealed record SpecCheckReport(
    SemanticVersion SchemaVersion,
    int SpecificationCount,
    IReadOnlyList<SpecReportLayout> Layouts,
    int ItemCount,
    int RulesEvaluated,
    IReadOnlyList<SpecViolation> Violations)
{
    /// <summary>How many violations fail the target.</summary>
    public int ErrorCount => Violations.Count(static violation => violation.Severity == SpecSeverity.Error);

    /// <summary>How many violations are reported without failing the target.</summary>
    public int WarningCount => Violations.Count(static violation => violation.Severity == SpecSeverity.Warning);
}
