namespace Specht.Tests.Identifiers;

/// <summary>
/// A per-Feature type and a name its Feature holds an id of that type by (<c>0001-F19</c> B-040 to B-042): a file name in
/// the Feature's <c>.spec/decisions/</c>, the first cell of a table row in its <c>.spec/README.md</c>, and a scenario tag in its
/// companion, less the <c>@</c>.
/// </summary>
public sealed class PerFeaturePlaces : TheoryData<string, string>
{
    /// <summary>Initializes one row per per-Feature type.</summary>
    public PerFeaturePlaces()
    {
        Add("decision", "abc123-held.md");
        Add("claim", "B-abc123");
        Add("constraint", "C-abc123");
        Add("openQuestion", "OQ-abc123");
        Add("scenario", "S-abc123");
    }
}
