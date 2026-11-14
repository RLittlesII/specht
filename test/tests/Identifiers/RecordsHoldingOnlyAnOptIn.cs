namespace Specht.Tests.Identifiers;

/// <summary>
/// A type, the strategy its only <c>ids</c> record opts it in to with no size and no encoding under schema <c>0.4.0</c>, the
/// frontmatter of <c>features/one</c>'s specification, and the Feature a per-Feature type is minted for (<c>0001-F19</c>
/// B-074; item <c>0437</c>): every type under each strategy for a Feature that declares no prefix, and claim ids under Nano ID
/// for a Feature that declares one.
/// </summary>
public sealed class RecordsHoldingOnlyAnOptIn : TheoryData<string, string, string, string?>
{
    /// <summary>Initializes one row per type and strategy, and the prefixed claim row.</summary>
    public RecordsHoldingOnlyAnOptIn()
    {
        foreach (var strategy in new[] { "nanoid", "snowflake" })
        {
            Add("adr", strategy, string.Empty, null);
            Add("lesson", strategy, string.Empty, null);
            Add("decision", strategy, string.Empty, "features/one");
            Add("claim", strategy, string.Empty, "features/one");
            Add("constraint", strategy, string.Empty, "features/one");
            Add("openQuestion", strategy, string.Empty, "features/one");
            Add("scenario", strategy, string.Empty, "features/one");
        }

        Add("claim", "nanoid", "---\nclaim_prefix: \"ACTIVITY\"\n---\n", "features/one");
    }
}
