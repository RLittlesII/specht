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

    public const string CollectedFullJson = """
        {
          "Title": "Synthetic workload",
          "HostEnvironmentInfo": {
            "BenchmarkDotNetCaption": "BenchmarkDotNet", "BenchmarkDotNetVersion": "0.15.8", "OsVersion": "Synthetic OS 1.0",
            "ProcessorName": "Synthetic CPU", "PhysicalProcessorCount": 1, "PhysicalCoreCount": 4, "LogicalCoreCount": 8,
            "RuntimeVersion": ".NET 10.0.0", "Architecture": "X64", "HasAttachedDebugger": false, "HasRyuJit": true,
            "Configuration": "RELEASE", "DotNetCliVersion": "11.0.100-synthetic.1", "ChronometerFrequency": { "Hertz": 1000000000 },
            "HardwareTimerKind": "Unknown"
          },
          "Benchmarks": [{
            "DisplayInfo": "CheckBenchmarks.Check: ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3) [Specifications=1]",
            "FullName": "Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)", "Parameters": "Specifications=1",
            "Statistics": { "Mean": 100, "ConfidenceInterval": { "Mean": 100, "Level": 12, "Lower": 98, "Upper": 102 } },
            "Memory": { "BytesAllocatedPerOperation": 32 },
            "Measurements": []
          }, {
            "DisplayInfo": "CheckBenchmarks.Check: ShortRun(Toolchain=.NET 10.0, IterationCount=3, LaunchCount=1, WarmupCount=3) [Specifications=10]",
            "FullName": "Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 10)", "Parameters": "Specifications=10",
            "Statistics": { "Mean": 900, "ConfidenceInterval": { "Mean": 900, "Level": 12, "Lower": 890, "Upper": 910 } },
            "Memory": { "BytesAllocatedPerOperation": 320 },
            "Measurements": []
          }]
        }
        """;
}
