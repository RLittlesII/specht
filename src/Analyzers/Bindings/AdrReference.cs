namespace Specht.Analysis.Bindings;

/// <summary>An explicit ADR reference; an absent Feature selects repository ownership.</summary>
public sealed record AdrReference(string? Epic, string? Feature, string? Adr);
