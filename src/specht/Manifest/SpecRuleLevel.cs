namespace Specht.Manifest;

/// <summary>A rule's own selection level or an explicit manifest override.</summary>
public enum SpecRuleLevel
{
    /// <summary>Report an error.</summary>
    Error,
    /// <summary>Report a warning.</summary>
    Warning,
    /// <summary>Disable the rule id.</summary>
    Off,
}
