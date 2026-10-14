namespace Specht.Comparison.Tests.Evidence;

internal static class ExportData
{
    public const string FullJson = """
        {
          "Title": "Synthetic workload",
          "HostEnvironmentInfo": { "BenchmarkDotNetVersion": "0.15.8", "Configuration": "Release", "HasAttachedDebugger": false },
          "Benchmarks": [{
            "FullName": "Synthetic.Check(1)", "DisplayInfo": "check", "Parameters": "Specifications=1",
            "Statistics": { "Mean": 100, "ConfidenceInterval": { "Mean": 100, "Level": 12, "Lower": 98, "Upper": 102 } },
            "Memory": { "BytesAllocatedPerOperation": 32 },
            "Measurements": []
          }]
        }
        """;
}
