namespace Specht.Tests.Evidence;

/// <summary>
/// A mapped claim record's <c>plain</c> and <c>id</c>, a claim, and whether <c>specht evidence</c> reads it as well formed
/// (<c>0201-F5</c> B-045): the head then the id, another head, a partial match of the id, a head in another case, a head
/// whose punctuation is literal, and an alternation matched whole.
/// </summary>
public sealed class EvidenceClaimRecordMatches : TheoryData<string, string, string, bool>
{
    /// <summary>Initializes the rows.</summary>
    public EvidenceClaimRecordMatches()
    {
        Add("need/{id}", "[0-9]+", "need/7", true);
        Add("need/{id}", "[0-9]+", "want/7", false);
        Add("need/{id}", "[0-9]+", "need/7a", false);
        Add("need/{id}", "[0-9]+", "NEED/7", false);
        Add("need.{id}", "[0-9]+", "need.7", true);
        Add("need.{id}", "[0-9]+", "needx7", false);
        Add("need/{id}", "[0-9]+|[a-z]{2}", "need/ab", true);
        Add("need/{id}", "[0-9]+|[a-z]{2}", "need/7ab", false);
    }
}
