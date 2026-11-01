namespace Specht.Comparison;

/// <summary>The paired session and the outcome of each revision.</summary>
public sealed record CollectionResult(
    SessionManifest Session,
    RevisionCollection Previous,
    RevisionCollection Current);
