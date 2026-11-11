namespace Specht.Tests.Identifiers;

/// <summary>
/// A per-Feature type whose first candidate, its in-tree prefix (B-055) ahead of <c>x</c>, the Feature holds in that type's
/// place, and the next candidate, the same prefix ahead of <c>y</c> (<c>0001-F19</c> B-010, B-040 to B-042).
/// </summary>
public sealed class PerFeatureFirstCandidatesHeld : TheoryData<string, string>
{
    /// <summary>Initializes one row per per-Feature type.</summary>
    public PerFeatureFirstCandidatesHeld()
    {
        Add("decision", "y");
        Add("claim", "B-y");
        Add("constraint", "C-y");
        Add("openQuestion", "OQ-y");
        Add("scenario", "S-y");
    }
}
