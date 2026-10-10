namespace specht;

/// <summary>The outcome of one <c>SpecCheck</c> run.</summary>
/// <param name="SchemaVersion">The schema version the check ran under (<c>0001-F7</c> B-002).</param>
/// <param name="SpecificationCount">How many specifications were discovered, across both layouts.</param>
/// <param name="LegacyCount">How many are still at <c>epics/**/spec.md</c>.</param>
/// <param name="CoLocatedCount">How many have been migrated to <c>**/.spec/README.md</c>.</param>
/// <param name="ItemCount">How many task, test, bug and spike files sit beside them.</param>
/// <param name="RulesEvaluated">How many <c>SPEC###</c> ids were evaluated.</param>
/// <param name="Violations">Every violation, most severe first.</param>
public sealed record SpecCheckReport(
    int SchemaVersion,
    int SpecificationCount,
    int LegacyCount,
    int CoLocatedCount,
    int ItemCount,
    int RulesEvaluated,
    IReadOnlyList<SpecViolation> Violations)
{
    /// <summary>How many violations fail the target.</summary>
    public int ErrorCount => Violations.Count(static violation => violation.Severity == SpecSeverity.Error);

    /// <summary>How many violations are reported without failing the target.</summary>
    public int WarningCount => Violations.Count(static violation => violation.Severity == SpecSeverity.Warning);

    /// <summary>The migration tally, logged on every run so the number is never a question.</summary>
    public string MigrationSummary =>
        $"{SpecificationCount} specification(s) - {LegacyCount} legacy (epics/**/spec.md), "
            + $"{CoLocatedCount} co-located (**/.spec/README.md). "
            + $"Migration {(SpecificationCount == 0 ? 0 : CoLocatedCount * 100 / SpecificationCount)}% complete.";
}
