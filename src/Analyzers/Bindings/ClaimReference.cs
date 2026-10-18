namespace Specht.Analysis.Bindings;

/// <summary>An explicit persisted claim reference without inherited authoring context.</summary>
public sealed record ClaimReference(string? Epic, string? Feature, string? Claim);
