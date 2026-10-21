using System.Collections.ObjectModel;

namespace Specht.Manifest;

/// <summary>The validated rule levels, copied from the manifest once.</summary>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed class SpecRuleSettings : ReadOnlyDictionary<string, SpecRuleLevel>
{
    /// <summary>Copies the explicitly declared levels, with ordinal rule ids.</summary>
    /// <param name="levels">The validated levels.</param>
    /// <implements epic="0001" feature="F5" claim="B-048" />
    public SpecRuleSettings(IEnumerable<KeyValuePair<string, SpecRuleLevel>> levels)
        : base(new Dictionary<string, SpecRuleLevel>(levels, StringComparer.Ordinal)) { }
}
