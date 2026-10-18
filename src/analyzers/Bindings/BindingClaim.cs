namespace Specht.Analysis.Bindings;

/// <summary>A declared claim with its original lifecycle and provenance.</summary>
public sealed record BindingClaim(
    string Id,
    string? State,
    int Line = 1,
    string? ReplacementId = null,
    string? CoverageExemption = null);
