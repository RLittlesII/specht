namespace Specht.Comparison;

/// <summary>Immutable Observation evidence for release comparison.</summary>
public sealed record Observation(
    MetricKind Metric,
    decimal? Value,
    string Unit,
    string Observer,
    TimingInterval? Interval,
    string? InvalidReason,
    string SourceFile,
    string Locator);
