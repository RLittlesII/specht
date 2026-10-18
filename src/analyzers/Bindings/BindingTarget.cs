namespace Specht.Analysis.Bindings;

/// <summary>A selected target retaining its effective identity, raw lifecycle and source provenance.</summary>
public sealed record BindingTarget(
    BindingTargetKind Kind,
    string? Epic,
    string? Feature,
    string Id,
    string? State,
    string? FeatureState,
    string RelativePath,
    int Line);
