namespace Specht.Evaluation;

/// <summary>The order the report carries its violations in (<c>0001-F1</c> B-004, C-9; ADR-0010 § "The evaluator is pure").</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtViolationOrder
{
    /// <summary>
    /// Orders <paramref name="violations"/>: severity descending, then file, line and rule id, file and rule id compared
    /// ordinally, and violations equal on all four keys kept in the order they were given.
    /// </summary>
    /// <param name="violations">The violations to order.</param>
    /// <returns>The violations, ordered.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public IReadOnlyList<SpechtViolation> Order(IEnumerable<SpechtViolation> violations) =>
        violations
            .OrderByDescending(static violation => violation.Severity)
            .ThenBy(static violation => violation.File, StringComparer.Ordinal)
            .ThenBy(static violation => violation.Line)
            .ThenBy(static violation => violation.RuleId, StringComparer.Ordinal)
            .ToList();
}
