namespace specht;

/// <summary>
/// One specification rule. Implementations are discovered by reflection over
/// this assembly, so adding a rule means adding a file and nothing else.
/// </summary>
public interface ISpecRule
{
    /// <summary>The rule family's primary id, e.g. <c>SPEC03x</c>'s <c>SPEC030</c>.</summary>
    string Id { get; }

    /// <summary>Every <c>SPEC###</c> id this rule can report, for the summary line.</summary>
    IReadOnlyList<string> ReportedIds { get; }

    /// <summary>Evaluates the rule against the whole resolved tree.</summary>
    IEnumerable<SpecViolation> Evaluate(SpecModel model);
}
