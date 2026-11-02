using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC052 - no Feature depends on itself or on a cycle.</summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DependencyCycleRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC052";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var features = model.FeaturesWithIdentity;
        var graph = model.DependencyGraph;

        foreach (var feature in features)
        {
            foreach (var (key, edges) in new[] { ("depends_on", graph.DependsOn[feature.Identity]), ("blocks", graph.Blocks[feature.Identity]) })
            {
                foreach (var edge in edges)
                {
                    if (!graph.Identities.Contains(edge) || !string.Equals(edge, feature.Identity, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    yield return new SpechtFinding(
                        feature.RelativePath,
                        feature.Document.Frontmatter.LineOf(key),
                        edge,
                        $"frontmatter {key} names this Feature itself")
                    {
                        Expected = new JsonObject { ["cycle"] = new JsonArray(feature.Identity, feature.Identity) },
                    };
                }
            }
        }

        foreach (var finding in CheckCycles(features, graph.DependsOn))
        {
            yield return finding;
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static IEnumerable<SpechtFinding> CheckCycles(
        IReadOnlyList<FeatureSpec> features,
        IReadOnlyDictionary<string, IReadOnlySet<string>> dependsOn)
    {
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var feature in features)
        {
            var path = new List<string>();

            if (!HasCycle(feature.Identity, dependsOn, [], path) || !reported.Add(feature.Identity))
            {
                continue;
            }

            yield return new SpechtFinding(
                feature.RelativePath,
                feature.Document.Frontmatter.LineOf("depends_on"),
                feature.Identity,
                $"dependency cycle: {string.Join(" -> ", path)}")
            {
                Expected = new JsonObject
                {
                    ["cycle"] = new JsonArray(path.Select(static identity => (JsonNode?)JsonValue.Create(identity)).ToArray()),
                },
            };
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static bool HasCycle(
        string identity,
        IReadOnlyDictionary<string, IReadOnlySet<string>> dependsOn,
        HashSet<string> visiting,
        List<string> path)
    {
        if (!visiting.Add(identity))
        {
            path.Add(identity);

            return true;
        }

        path.Add(identity);

        if (dependsOn.TryGetValue(identity, out var edges))
        {
            foreach (var edge in edges.Order(StringComparer.Ordinal))
            {
                if (HasCycle(edge, dependsOn, visiting, path))
                {
                    return true;
                }
            }
        }

        visiting.Remove(identity);
        path.RemoveAt(path.Count - 1);

        return false;
    }
}
