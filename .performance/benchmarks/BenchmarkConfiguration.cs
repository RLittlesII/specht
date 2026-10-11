using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace Specht.Benchmarks;

/// <summary>The configuration every benchmark in this assembly runs under.</summary>
public sealed class BenchmarkConfiguration : ManualConfig
{
    /// <summary>Initializes a new instance of the <see cref="BenchmarkConfiguration"/> class.</summary>
    public BenchmarkConfiguration()
    {
        var settings = NetCoreAppSettings.NetCoreApp10_0;
        var toolchain = new Toolchain(
            settings.Name,
            new BenchmarkProjectGenerator(settings),
            new DotNetCliBuilder(settings.TargetFrameworkMoniker, settings.CustomDotNetCliPath),
            new DotNetCliExecutor(settings.CustomDotNetCliPath!));

        AddLogger(ConsoleLogger.Default);
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray());
        AddValidator(DefaultConfig.Instance.GetValidators().ToArray());
        AddDiagnoser(MemoryDiagnoser.Default);
        AddExporter(MarkdownExporter.GitHub, JsonExporter.Full);
        AddJob(Job.Default.WithToolchain(toolchain).AsMutator());
    }
}
