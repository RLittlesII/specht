using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Specht.Benchmarks.Startup;

/// <summary>Measures installed-tool invocations; allocations belong to the benchmark process (0109-F4 B-001 to B-005, B-007 to B-009).</summary>
public class ColdStartBenchmarks
{
    /// <summary>Installs both local tools and verifies the one-specification input outside measurement.</summary>
    /// <returns>The setup operation.</returns>
    [GlobalSetup]
    public async Task InstallTools() => _session = await ColdStartSession.CreateAsync();

    /// <summary>Removes the tools, manifest and private caches.</summary>
    [GlobalCleanup]
    public void DeleteSession() => _session?.Dispose();

    /// <summary>Starts the generated empty tool through the local manifest.</summary>
    /// <returns>The tool's output.</returns>
    [Benchmark(Baseline = true, Description = "Empty tool (Allocated: benchmark process)")]
    public Task<string> EmptyTool() => _session!.RunAsync("specht-empty");

    /// <summary>Starts the installed tool's help through the local manifest.</summary>
    /// <returns>The tool's output.</returns>
    [Benchmark(Description = "Help (Allocated: benchmark process)")]
    public Task<string> Help() => _session!.RunAsync("specht", "--help");

    /// <summary>Starts one check through the local manifest.</summary>
    /// <returns>The tool's output.</returns>
    [Benchmark(Description = "Check (Allocated: benchmark process)")]
    public Task<string> Check() => _session!.RunAsync("specht", "--root", "tree");

    private ColdStartSession? _session;
}
