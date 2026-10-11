namespace Specht;

/// <summary>The closed set of input failures a check returns in place of a report.</summary>
public enum SpechtFailureKind
{
    /// <summary>The root is not a directory (<c>0001-F2</c> B-005).</summary>
    RootNotFound,

    /// <summary>The root holds no manifest at the manifest path (<c>0001-F2</c> B-006).</summary>
    ManifestNotFound,

    /// <summary>The manifest is not well-formed JSON or not the manifest's shape (<c>0001-F2</c> B-007).</summary>
    ManifestUnreadable,

    /// <summary>The manifest is well-formed and the engine rejects it (<c>0001-F5</c> B-012).</summary>
    ManifestRejected,
}
