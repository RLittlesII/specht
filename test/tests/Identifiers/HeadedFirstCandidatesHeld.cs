namespace Specht.Tests.Identifiers;

/// <summary>
/// A per-Feature type, the form of its <c>ids</c> record under schema <c>0.4.0</c>, and the id minted for a Feature that
/// declares the claim prefix <c>ACTIVITY</c> and holds the first candidate, the head ahead of <c>aa</c>, in that type's place:
/// the next candidate, the head ahead of <c>bb</c> (<c>0001-F19</c> B-070, B-071; item <c>0437</c>).
/// </summary>
public sealed class HeadedFirstCandidatesHeld : TheoryData<string, string, string>
{
    /// <summary>Initializes the claim, constraint, open question and scenario rows.</summary>
    public HeadedFirstCandidatesHeld()
    {
        Add("claim", "{prefix}-{id}", "ACTIVITY-bb");
        Add("constraint", "{prefix}-C-{id}", "ACTIVITY-C-bb");
        Add("openQuestion", "{prefix}-Q-{id}", "ACTIVITY-Q-bb");
        Add("scenario", "{prefix}-S-{id}", "ACTIVITY-S-bb");
    }
}
