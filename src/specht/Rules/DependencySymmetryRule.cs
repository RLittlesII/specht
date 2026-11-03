using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC051 - every dependency edge between two Features is declared from both ends.</summary>
/// <remarks>
/// <c>blocks</c> is a derived field: it is the reverse of some other Feature's
/// <c>depends_on</c>. Checking the symmetry is what makes "derived" mean
/// something, since nothing recomputes it automatically.
/// </remarks>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DependencySymmetryRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC051";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var graph = model.DependencyGraph;

        foreach (var feature in model.FeaturesWithIdentity)
        {
            foreach (var (key, opposite, edges, reverse) in new[]
            {
                (feature.Keys.DependsOn, feature.Keys.Blocks, graph.DependsOn[feature.Identity], graph.Blocks),
                (feature.Keys.Blocks, feature.Keys.DependsOn, graph.Blocks[feature.Identity], graph.DependsOn),
            })
            {
                foreach (var edge in edges)
                {
                    if (!graph.Identities.Contains(edge) || string.Equals(edge, feature.Identity, StringComparison.Ordinal)
                        || !reverse.TryGetValue(edge, out var declared) || declared.Contains(feature.Identity))
                    {
                        continue;
                    }

                    yield return new SpechtFinding(
                        feature.RelativePath,
                        feature.Document.Frontmatter.LineOf(key),
                        edge,
                        $"'{feature.Identity}' declares {key} '{edge}', but '{edge}' does not declare {opposite} '{feature.Identity}'")
                    {
                        Expected = new JsonObject
                        {
                            ["edge"] = new JsonObject { ["feature"] = edge, ["key"] = opposite, ["value"] = feature.Identity },
                        },
                    };
                }
            }
        }
    }
}
