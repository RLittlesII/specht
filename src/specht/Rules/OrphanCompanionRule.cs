using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC024 - a companion file sits beside a specification, not in a specification's place without one.</summary>
/// <implements epic="0001" feature="F6" claim="B-013" />
public sealed class OrphanCompanionRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC024";

    /// <inheritdoc />
    public SpechtRuleLevel DefaultLevel => SpechtRuleLevel.Warning;

    /// <inheritdoc />
    /// <implements epic="0001" feature="F6" claim="B-013" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model) =>
        model.OrphanCompanions.Select(static path => new SpechtFinding(path, 0, null, "no specification sits beside this companion file"));
}
