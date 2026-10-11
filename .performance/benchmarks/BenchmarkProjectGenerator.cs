using System;
using System.IO;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

namespace Specht.Benchmarks;

internal sealed class BenchmarkProjectGenerator(NetCoreAppSettings settings)
    : CsProjGenerator(
        settings.TargetFrameworkMoniker,
        settings.CustomDotNetCliPath!,
        settings.PackagesPath!,
        settings.RuntimeFrameworkVersion!)
{
    protected override FileInfo GetProjectFilePath(Type benchmarkTarget, ILogger logger)
    {
        for (var directory = new FileInfo(benchmarkTarget.Assembly.Location).Directory; directory is not null; directory = directory.Parent)
        {
            var project = new FileInfo(Path.Combine(directory.FullName, "benchmarks.csproj"));
            if (project.Exists)
            {
                return project;
            }
        }

        return base.GetProjectFilePath(benchmarkTarget, logger);
    }
}
