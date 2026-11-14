namespace Specht.Trace;

/// <summary>A named test's resolution in the trace, one of A-1's four values (<c>0001-F13</c> B-003, B-019).</summary>
/// <implements epic="0001" feature="F13" claim="B-003" />
/// <implements epic="0001" feature="F13" claim="B-019" />
public enum TraceResolution
{
    /// <summary>A declared test file holds the name in its required form.</summary>
    Resolved,

    /// <summary>No declared test file holds the name in its required form.</summary>
    Unresolved,

    /// <summary>The manifest declares no test files.</summary>
    Unconfigured,

    /// <summary>The row's Status is the waived, withdrawn or superseded marker, so its tests are not resolved.</summary>
    Skipped,
}
