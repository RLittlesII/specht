namespace Specht.Model;

/// <summary>An epic file and its frontmatter.</summary>
/// <param name="RelativePath">Path relative to the repository root.</param>
/// <param name="Frontmatter">Its frontmatter.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed record EpicFile(string RelativePath, Frontmatter Frontmatter)
{
    /// <summary>The <c>id</c> frontmatter value, or <c>null</c>.</summary>
    public string? Id => Frontmatter.Text("id");
}
