using System.Collections.ObjectModel;

namespace Specht.Manifest;

/// <summary>The validated rule levels, copied from the manifest once.</summary>
public sealed class SpecRuleSettings : ReadOnlyDictionary<string, SpecRuleLevel>
{
    /// <summary>Copies the explicitly declared levels, with ordinal rule ids.</summary>
    /// <param name="levels">The validated levels.</param>
    public SpecRuleSettings(IEnumerable<KeyValuePair<string, SpecRuleLevel>> levels)
        : base(new Dictionary<string, SpecRuleLevel>(levels, StringComparer.Ordinal)) { }
}
