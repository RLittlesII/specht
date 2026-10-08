namespace specht.Rules;

/// <summary>
/// SPEC050-SPEC052 - every dependency edge names a real Feature, every edge is
/// declared from both ends, and nothing depends on itself or on a cycle.
/// </summary>
/// <remarks>
/// <c>blocks</c> is a derived field: it is the reverse of some other Feature's
/// <c>depends_on</c>. Checking the symmetry is what makes "derived" mean
/// something, since nothing recomputes it automatically.
/// </remarks>
public sealed class DependencyRule : ISpecRule
{
    /// <inheritdoc />
    public string Id => "SPEC050";

    /// <inheritdoc />
    public IReadOnlyList<string> ReportedIds => ["SPEC050", "SPEC051", "SPEC052"];

    /// <inheritdoc />
    public IEnumerable<SpecViolation> Evaluate(SpecModel model)
    {
        var features = model.Features
            .Where(static feature => feature.Epic is not null && feature.Id is not null)
            .ToList();

        var identities = features.Select(static feature => feature.Identity).ToHashSet(StringComparer.Ordinal);
        var dependsOn = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var blocks = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        foreach (var feature in features)
        {
            dependsOn[feature.Identity] = Resolve(feature, "depends_on");
            blocks[feature.Identity] = Resolve(feature, "blocks");
        }

        foreach (var feature in features)
        {
            foreach (var violation in CheckEdges(feature, identities, dependsOn, blocks))
            {
                yield return violation;
            }
        }

        foreach (var violation in CheckCycles(features, dependsOn))
        {
            yield return violation;
        }
    }

    /// <summary>Expands an edge to its qualified <c>&lt;epic&gt;-F&lt;n&gt;</c> form.</summary>
    /// <remarks>
    /// An edge is written either locally (<c>F2</c>) or qualified
    /// (<c>0002/F1</c>), and both mean the same thing.
    /// </remarks>
    private static HashSet<string> Resolve(FeatureSpec feature, string key)
    {
        var resolved = new HashSet<string>(StringComparer.Ordinal);

        foreach (var raw in feature.Strings(key))
        {
            resolved.Add(raw.Contains('/') ? raw.Replace("/", "-") : $"{feature.Epic}-{raw}");
        }

        return resolved;
    }

    private static IEnumerable<SpecViolation> CheckEdges(
        FeatureSpec feature,
        IReadOnlySet<string> identities,
        IReadOnlyDictionary<string, HashSet<string>> dependsOn,
        IReadOnlyDictionary<string, HashSet<string>> blocks)
    {
        foreach (var (key, edges, reverse) in Edges(feature, dependsOn, blocks))
        {
            foreach (var edge in edges)
            {
                if (!identities.Contains(edge))
                {
                    yield return new SpecViolation(
                        "SPEC050",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        feature.Document.Frontmatter.LineOf(key),
                        edge,
                        $"frontmatter {key} names '{edge}', which is not a discovered Feature");

                    continue;
                }

                if (string.Equals(edge, feature.Identity, StringComparison.Ordinal))
                {
                    yield return new SpecViolation(
                        "SPEC052",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        feature.Document.Frontmatter.LineOf(key),
                        edge,
                        $"frontmatter {key} names this Feature itself");

                    continue;
                }

                if (reverse.TryGetValue(edge, out var declared) && !declared.Contains(feature.Identity))
                {
                    var opposite = key == "depends_on" ? "blocks" : "depends_on";

                    yield return new SpecViolation(
                        "SPEC051",
                        SpecSeverity.Error,
                        feature.RelativePath,
                        feature.Document.Frontmatter.LineOf(key),
                        edge,
                        $"'{feature.Identity}' declares {key} '{edge}', but '{edge}' does not declare {opposite} '{feature.Identity}'");
                }
            }
        }
    }

    private static IEnumerable<(string Key, HashSet<string> Edges, IReadOnlyDictionary<string, HashSet<string>> Reverse)> Edges(
        FeatureSpec feature,
        IReadOnlyDictionary<string, HashSet<string>> dependsOn,
        IReadOnlyDictionary<string, HashSet<string>> blocks)
    {
        yield return ("depends_on", dependsOn[feature.Identity], blocks);
        yield return ("blocks", blocks[feature.Identity], dependsOn);
    }

    private static IEnumerable<SpecViolation> CheckCycles(
        IReadOnlyList<FeatureSpec> features,
        IReadOnlyDictionary<string, HashSet<string>> dependsOn)
    {
        var reported = new HashSet<string>(StringComparer.Ordinal);

        foreach (var feature in features)
        {
            var path = new List<string>();

            if (!HasCycle(feature.Identity, dependsOn, [], path) || !reported.Add(feature.Identity))
            {
                continue;
            }

            yield return new SpecViolation(
                "SPEC052",
                SpecSeverity.Error,
                feature.RelativePath,
                feature.Document.Frontmatter.LineOf("depends_on"),
                feature.Identity,
                $"dependency cycle: {string.Join(" -> ", path)}");
        }
    }

    private static bool HasCycle(
        string identity,
        IReadOnlyDictionary<string, HashSet<string>> dependsOn,
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
