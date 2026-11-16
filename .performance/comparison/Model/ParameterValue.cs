namespace Specht.Comparison.Model;

/// <summary>Immutable ParameterValue evidence for release comparison.</summary>
public sealed record ParameterValue(
    string Kind,
    string Value);
