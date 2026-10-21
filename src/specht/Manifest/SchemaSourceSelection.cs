namespace Specht.Manifest;

/// <summary>The explicit frontmatter schema source selected by a manifest or one check invocation.</summary>
/// <implements epic="0001" feature="F7" claim="B-009" />
public enum SchemaSourceSelection
{
    /// <summary>The frontmatter schemas embedded for the pinned version.</summary>
    Embedded,

    /// <summary>The named local schemas, retaining verification when an upstream source is recorded.</summary>
    Disk,
}
