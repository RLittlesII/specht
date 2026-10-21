namespace Specht.Model;

/// <summary>Where the schemas a check used came from (B-005).</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public enum SpecSchemaSource
{
    /// <summary>The set embedded in the tool.</summary>
    Embedded,

    /// <summary>The repository's own files under <c>.specht/schema/frontmatter/</c>.</summary>
    Disk,

    /// <summary>The on-disk copy of a recorded upstream source.</summary>
    Upstream,
}
