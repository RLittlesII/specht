namespace Specht.Comparison;

/// <summary>Immutable ParameterValue evidence for release comparison.</summary>
public sealed record ParameterValue(
    string Kind,
    string Value);
