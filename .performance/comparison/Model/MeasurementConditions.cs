namespace Specht.Comparison;

/// <summary>Immutable MeasurementConditions evidence for release comparison.</summary>
public sealed record MeasurementConditions(
    string? OperatingSystem,
    string? Runtime,
    string? Architecture,
    string? Toolchain,
    string? GarbageCollection,
    string? Job);
