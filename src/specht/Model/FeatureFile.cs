namespace Specht.Model;

/// <summary>A companion's root-relative path, claim tags and scenarios, read before evaluation.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class FeatureFile
{
    /// <summary>Copies a companion's parsed tags and scenarios.</summary>
    /// <param name="relativePath">The path relative to the checked root.</param>
    /// <param name="tags">The parsed tags in file order.</param>
    /// <param name="scenarios">The parsed scenarios in file order.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public FeatureFile(string relativePath, IReadOnlyList<FeatureTag> tags, IReadOnlyList<FeatureScenario> scenarios)
    {
        RelativePath = relativePath;
        Tags = Array.AsReadOnly(tags.ToArray());
        Scenarios = Array.AsReadOnly(scenarios.ToArray());
    }

    /// <summary>The path relative to the checked root.</summary>
    public string RelativePath { get; }

    /// <summary>The parsed tags in file order.</summary>
    public IReadOnlyList<FeatureTag> Tags { get; }

    /// <summary>The <c>Scenario:</c> and <c>Scenario Outline:</c> entries in file order.</summary>
    public IReadOnlyList<FeatureScenario> Scenarios { get; }
}
