using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC050 - every dependency edge names a discovered Feature.</summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DependencyTargetRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC050";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var graph = model.DependencyGraph;

        foreach (var feature in model.FeaturesWithIdentity)
        {
            foreach (var (key, edges) in new[] { ("depends_on", graph.DependsOn[feature.Identity]), ("blocks", graph.Blocks[feature.Identity]) })
            {
                foreach (var edge in edges)
                {
                    if (graph.Identities.Contains(edge))
                    {
                        continue;
                    }

                    yield return new SpechtFinding(
                        feature.RelativePath,
                        feature.Document.Frontmatter.LineOf(key),
                        edge,
                        $"frontmatter {key} names '{edge}', which is not a discovered Feature")
                    {
                        Expected = new JsonObject
                        {
                            ["features"] = new JsonArray(graph.Identities.Order(StringComparer.Ordinal)
                                .Select(static identity => (JsonNode?)JsonValue.Create(identity)).ToArray()),
                        },
                    };
                }
            }
        }
    }
}
