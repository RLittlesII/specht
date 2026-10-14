namespace Specht.Comparison;

/// <summary>Immutable TimingInterval evidence for release comparison.</summary>
public sealed record TimingInterval(
    decimal Lower,
    decimal Upper,
    decimal ConfidenceLevel);
