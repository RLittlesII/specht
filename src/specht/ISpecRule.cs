namespace Specht;

/// <summary>
/// One specification rule. Implementations are discovered by reflection over
/// this assembly until ADR-0001 replaces that with an explicit list. A new
/// rule is a new schema version (brief § 4), not just a new file.
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
