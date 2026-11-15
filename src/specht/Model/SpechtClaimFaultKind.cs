namespace Specht.Model;

/// <summary>Why the check could not take claims from a piece of claim input (<c>0001-F16</c> B-014 to B-016, B-018, B-019, B-033, B-034).</summary>
/// <implements epic="0001" feature="F16" claim="B-014" />
public enum SpechtClaimFaultKind
{
    /// <summary>The shared Gherkin reader refused the claim file's text (B-014, B-015).</summary>
    /// <implements epic="0001" feature="F16" claim="B-014" />
    /// <implements epic="0001" feature="F16" claim="B-015" />
    UnreadableFile,

    /// <summary>The claim file set's glob matches no claim file (B-016).</summary>
    /// <implements epic="0001" feature="F16" claim="B-016" />
    EmptySet,

    /// <summary>The claim file matches more than one claim file set (B-018).</summary>
    /// <implements epic="0001" feature="F16" claim="B-018" />
    SharedFile,

    /// <summary>A Feature-level tag names a Feature other than its claim file set's (B-019).</summary>
    /// <implements epic="0001" feature="F16" claim="B-019" />
    FeatureTag,

    /// <summary>The claim file set's <c>record</c> names no file, one that cannot be read, or one outside the root (B-033).</summary>
    /// <implements epic="0001" feature="F16" claim="B-033" />
    UnreadableRecord,

    /// <summary>The claim file set's <c>record</c> is a rooted path (B-034).</summary>
    /// <implements epic="0001" feature="F16" claim="B-034" />
    AbsoluteRecord,
}
