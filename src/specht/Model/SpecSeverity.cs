namespace Specht.Model;

/// <summary>How much a violation matters to the build.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public enum SpecSeverity
{
    /// <summary>Reported, but the target still succeeds.</summary>
    Warning,

    /// <summary>Fails the target.</summary>
    Error,
}
