using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC040 - each id a Feature's <c>children</c> declares resolves to one item file beside the specification.</summary>
/// <param name="check">The declared-item check SPEC040 shares with SPEC041.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DeclaredChildrenRule(DeclaredItemCheck check) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC040";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        var byDirectory = model.Items
            .GroupBy(static item => item.ParentDirectory, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.ToList(), StringComparer.Ordinal);

        foreach (var feature in model.Features)
        {
            var siblings = byDirectory.TryGetValue(feature.Location.Directory, out var items) ? items : [];

            foreach (var finding in check.Evaluate(feature, siblings, feature.Keys.Children, null))
            {
                yield return finding;
            }
        }
    }
}
