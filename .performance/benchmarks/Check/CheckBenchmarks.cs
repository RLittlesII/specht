using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace Specht.Benchmarks.Check;

/// <summary>Measures the engine's whole check over generated trees of four sizes (<c>0109-F3</c> B-001 to B-004; C-1 to C-4).</summary>
public class CheckBenchmarks
{
    /// <summary>Gets or sets how many specifications the measured tree holds.</summary>
    [Params(1, 10, 100, 1000)]
    public int Specifications { get; set; }

    /// <summary>Generates this size's tree and refuses a tree the check does not count whole and clean.</summary>
    [GlobalSetup]
    public void GenerateTree()
    {
        _root = Directory.CreateTempSubdirectory("specht-check-").FullName;
        SpecTreeGenerator.Generate(_root, Specifications);

        var report = SpechtRunner.Run(_root);
        if (report.SpecificationCount == Specifications && report.Violations.Count == 0)
        {
            return;
        }

        Directory.Delete(_root, recursive: true);
        throw new InvalidOperationException(
            $"The check counted {report.SpecificationCount} of {Specifications} specifications and reported {report.Violations.Count} violations.");
    }

    /// <summary>Deletes this size's tree.</summary>
    [GlobalCleanup]
    public void DeleteTree() => Directory.Delete(_root, recursive: true);

    /// <summary>Runs the whole check from the tree's root.</summary>
    /// <returns>The report.</returns>
    [Benchmark]
    public SpechtReport Check() => SpechtRunner.Run(_root);

    private string _root = string.Empty;
}
