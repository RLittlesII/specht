namespace Specht.Comparison;

/// <summary>Recognised dimensional scales, without delta arithmetic or a review policy.</summary>
public static class MetricUnits
{
    public static bool IsRecognised(MetricKind metric, string unit) => Scale(metric, unit) is not null;

    public static decimal? Scale(MetricKind metric, string unit) => metric switch
    {
        MetricKind.Time => unit switch { "ns" => 1m, "us" or "µs" => 1000m, "ms" => 1000000m, "s" => 1000000000m, _ => null },
        MetricKind.ManagedAllocation or MetricKind.ToolPeakMemory => unit switch
        {
            "B" => 1m,
            "KB" => 1000m,
            "MB" => 1000000m,
            "KiB" => 1024m,
            "MiB" => 1048576m,
            _ => null,
        },
        _ => null,
    };
}
