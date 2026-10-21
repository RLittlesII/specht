namespace Specht.Model;

/// <summary>A companion's root-relative path and claim tags, read before evaluation.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class FeatureFile
{
    /// <summary>Copies a companion's parsed tags.</summary>
    /// <param name="relativePath">The path relative to the checked root.</param>
    /// <param name="tags">The parsed tags in file order.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public FeatureFile(string relativePath, IReadOnlyList<FeatureTag> tags)
    {
        RelativePath = relativePath;
        Tags = Array.AsReadOnly(tags.ToArray());
    }

    /// <summary>The path relative to the checked root.</summary>
    public string RelativePath { get; }

    /// <summary>The parsed tags in file order.</summary>
    public IReadOnlyList<FeatureTag> Tags { get; }
}
