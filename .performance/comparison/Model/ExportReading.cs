namespace Specht.Comparison;

/// <summary>The exporter version and measurement conditions a revision's exports agree on, or why they cannot be used.</summary>
public sealed record ExportReading(
    string? ExporterVersion,
    MeasurementConditions? Conditions,
    string? Failure);
