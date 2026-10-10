namespace Specht.Report;

/// <summary>Where the schemas a check used came from (B-005).</summary>
public enum SpecSchemaSource
{
    /// <summary>The set embedded in the tool.</summary>
    Embedded,

    /// <summary>The repository's own files under <c>.spec/schema/</c>.</summary>
    Disk,

    /// <summary>The on-disk copy of a recorded upstream source.</summary>
    Upstream,
}
