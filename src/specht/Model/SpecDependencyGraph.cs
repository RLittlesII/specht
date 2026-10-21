using System.Collections.ObjectModel;

namespace Specht.Model;

/// <summary>The qualified dependency edges of the Features with an identity, resolved once before rules run.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpecDependencyGraph
{
    /// <summary>Resolves both edge directions, preserving the existing duplicate-identity overwrite and edge order.</summary>
    /// <param name="features">Features with both epic and id, in discovery order.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecDependencyGraph(IReadOnlyList<FeatureSpec> features)
    {
        Identities = new ReadOnlySet<string>(features.Select(static feature => feature.Identity).ToHashSet(StringComparer.Ordinal));
        var dependsOn = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        var blocks = new Dictionary<string, IReadOnlySet<string>>(StringComparer.Ordinal);
        foreach (var feature in features)
        {
            dependsOn[feature.Identity] = Resolve(feature, "depends_on");
            blocks[feature.Identity] = Resolve(feature, "blocks");
        }

        DependsOn = new ReadOnlyDictionary<string, IReadOnlySet<string>>(dependsOn);
        Blocks = new ReadOnlyDictionary<string, IReadOnlySet<string>>(blocks);
    }

    /// <summary>The qualified identities.</summary>
    public IReadOnlySet<string> Identities { get; }

    /// <summary>Qualified depends_on edges by identity.</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> DependsOn { get; }

    /// <summary>Qualified blocks edges by identity.</summary>
    public IReadOnlyDictionary<string, IReadOnlySet<string>> Blocks { get; }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static IReadOnlySet<string> Resolve(FeatureSpec feature, string key)
    {
        var resolved = new HashSet<string>(StringComparer.Ordinal);
        foreach (var raw in feature.Strings(key))
        {
            resolved.Add(raw.Contains('/') ? raw.Replace("/", "-") : $"{feature.Epic}-{raw}");
        }

        return new ReadOnlySet<string>(resolved);
    }
}
