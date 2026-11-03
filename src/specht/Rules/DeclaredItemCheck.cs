using System.Text.Json.Nodes;
using Specht.Model;

namespace Specht.Rules;

/// <summary>
/// Resolves each id a Feature declares under one frontmatter key to the one item file that carries it, the check
/// SPEC040 and SPEC041 share (ADR-0010 § "One rule per id").
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DeclaredItemCheck
{
    /// <summary>
    /// Finds each id <paramref name="key"/> declares that no candidate or more than
    /// one candidate carries, or whose one candidate is not of <paramref name="requiredType"/>, in declaration order.
    /// </summary>
    /// <param name="feature">The Feature that declares the ids.</param>
    /// <param name="candidates">The items a declared id may resolve to.</param>
    /// <param name="key">The frontmatter key that declares them.</param>
    /// <param name="requiredType">The type the one match must carry, or <c>null</c> for any.</param>
    /// <returns>The findings.</returns>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    public IEnumerable<SpechtFinding> Evaluate(
        FeatureSpec feature,
        IReadOnlyCollection<ChildItem> candidates,
        string key,
        string? requiredType)
    {
        foreach (var declared in feature.Strings(key))
        {
            var matches = candidates
                .Where(item => string.Equals(item.FileNameId, declared, StringComparison.Ordinal))
                .ToList();

            if (matches.Count == 0)
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(key),
                    declared,
                    $"frontmatter {key} names '{declared}', but no '{declared}-*.md' was discovered")
                {
                    Expected = DeclaredExpectation(declared, requiredType),
                };

                continue;
            }

            if (matches.Count > 1)
            {
                yield return new SpechtFinding(
                    feature.RelativePath,
                    feature.Document.Frontmatter.LineOf(key),
                    declared,
                    $"frontmatter {key} names '{declared}', which resolves to {matches.Count} files")
                {
                    Expected = DeclaredExpectation(declared, requiredType),
                };

                continue;
            }

            if (requiredType is not null && !string.Equals(matches[0].Type, requiredType, StringComparison.Ordinal))
            {
                yield return new SpechtFinding(
                    matches[0].RelativePath,
                    matches[0].Frontmatter.LineOf(matches[0].Keys.Type),
                    declared,
                    $"'{declared}' is declared in frontmatter {key} but its type is '{matches[0].Type}', not '{requiredType}'")
                {
                    Expected = DeclaredExpectation(declared, requiredType),
                };
            }
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-001" />
    private static JsonObject DeclaredExpectation(string declared, string? requiredType)
    {
        var expected = new JsonObject { ["fileName"] = $"{declared}-*.md" };
        if (requiredType is not null)
        {
            expected["type"] = requiredType;
        }

        return expected;
    }
}
