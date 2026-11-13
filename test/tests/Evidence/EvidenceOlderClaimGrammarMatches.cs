namespace Specht.Tests.Evidence;

/// <summary>
/// A claim and whether <c>specht evidence</c> reads it as well formed when the manifest maps the older claim grammar
/// <c>^need/[0-9]+$</c> and no claim record (<c>0201-F5</c> B-046).
/// </summary>
public sealed class EvidenceOlderClaimGrammarMatches : TheoryData<string, bool>
{
    /// <summary>Initializes the rows.</summary>
    public EvidenceOlderClaimGrammarMatches()
    {
        Add("need/7", true);
        Add("want/7", false);
        Add("need/7a", false);
    }
}
