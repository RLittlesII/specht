namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceTarget(
    string Kind,
    string? Epic,
    string? Feature,
    string Id,
    string? State,
    string? FeatureState,
    string RelativePath,
    int Line);
