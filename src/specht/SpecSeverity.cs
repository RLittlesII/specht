namespace specht;

/// <summary>How much a violation matters to the build.</summary>
public enum SpecSeverity
{
    /// <summary>Reported, but the target still succeeds.</summary>
    Warning,

    /// <summary>Fails the target.</summary>
    Error,
}
