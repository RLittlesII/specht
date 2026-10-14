namespace Specht.Manifest;

/// <summary>An explicit rule override; absence retains the rule's severity.</summary>
public enum SpecRuleLevel
{
    /// <summary>Report an error.</summary>
    Error,
    /// <summary>Report a warning.</summary>
    Warning,
    /// <summary>Disable the rule id.</summary>
    Off,
}
