namespace Specht.Manifest;

/// <summary>A rule's own selection level or an explicit manifest override.</summary>
/// <implements epic="0001" feature="F5" claim="B-048" />
public enum SpecRuleLevel
{
    /// <summary>Report an error.</summary>
    Error,
    /// <summary>Report a warning.</summary>
    Warning,
    /// <summary>Disable the rule id.</summary>
    Off,
}
