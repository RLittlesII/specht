using Specht.Manifest;

namespace Specht.Rules;

/// <summary>One pinned rule's authoritative metadata and effective manifest level (0001-F10 B-001 to B-007).</summary>
/// <param name="Id">The id in the pinned version's vocabulary.</param>
/// <param name="Family">The family written on the rule page.</param>
/// <param name="DefaultSeverity">The rule page's default severity.</param>
/// <param name="EffectiveLevel">The explicit manifest level or the default severity.</param>
/// <implements epic="0001" feature="F10" claim="B-001" />
public sealed record RuleEntry(string Id, string Family, SpecRuleLevel DefaultSeverity, SpecRuleLevel EffectiveLevel);
