namespace Specht.Comparison;

/// <summary>The versioned strict review screens of decision 0002.</summary>
public sealed record ComparisonPolicy(
    int Version,
    decimal TimingIncreasePercent,
    decimal AllocationIncreasePercent)
{
    public static ComparisonPolicy Initial { get; } = new(1, 10m, 10m);
}
