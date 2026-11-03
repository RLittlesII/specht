using Specht.Model;

namespace Specht.Rules;

/// <summary>SPEC041 - each id a Feature's <c>spikes</c> declares resolves to one spike anywhere in the repository.</summary>
/// <param name="check">The declared-item check SPEC041 shares with SPEC040.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class DeclaredSpikesRule(DeclaredItemCheck check) : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC041";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F5" claim="B-004" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
    {
        foreach (var feature in model.Features)
        {
            foreach (var finding in check.Evaluate(feature, model.Items, feature.Keys.Spikes, "spike"))
            {
                yield return finding;
            }
        }
    }
}
