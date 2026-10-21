namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceTarget(
    string Kind,
    string? Epic,
    string? Feature,
    string Id,
    string? State,
    string? FeatureState,
    string RelativePath,
    int Line);
