namespace Specht.Analysis.Bindings;

/// <summary>A declared ADR whose null owner selects repository scope.</summary>
public sealed record BindingAdr(
    string Id,
    FeatureIdentity? Owner,
    string? State,
    string RelativePath,
    int Line = 1,
    string? ReplacementId = null);
