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
    public IReadOnlyList<string> ReportedIds => [Id];

    /// <inheritdoc />
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public IEnumerable<SpechtViolation> Evaluate(SpechtModel model)
    {
        foreach (var feature in model.Features)
        {
            foreach (var violation in check.Evaluate(feature, model.Items, "spikes", Id, "spike"))
            {
                yield return violation;
            }
        }
    }
}
