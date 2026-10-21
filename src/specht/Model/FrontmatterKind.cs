namespace Specht.Model;

/// <summary>The three frontmatter schema kinds named by the manifest.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public enum FrontmatterKind
{
    /// <summary>A Feature specification.</summary>
    Feature,
    /// <summary>A task-shaped document.</summary>
    Task,
    /// <summary>An epic document.</summary>
    Epic,
}
