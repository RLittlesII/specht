using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// One specification rule, under one id (ADR-0010 § "One rule per id"). The implementations that run are the ones
/// <c>AddSpechtEngine()</c> registers by name; none is found by reflection. A new rule is a new schema version
/// (brief § 4), not just a new file.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public interface ISpechtRule
{
    /// <summary>The <c>SPEC###</c> id every finding of this rule is reported under.</summary>
    string Id { get; }

    /// <summary>The rule's own level when the manifest names no override; existing rules remain enabled.</summary>
    SpechtRuleLevel DefaultLevel => SpechtRuleLevel.Error;

    /// <summary>Evaluates the rule against the whole resolved tree.</summary>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    IEnumerable<SpechtFinding> Evaluate(SpechtModel model);
}
