using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Evaluation;
using Specht.Tool;
using Specht.Tree;
using Spectre.Console.Cli;

namespace Specht.Benchmarks.Check;

/// <summary>Measures the engine's whole check over generated trees of four sizes (<c>0109-F3</c> B-001 to B-004; C-1 to C-4).</summary>
public class CheckBenchmarks
{
    /// <summary>Gets or sets how many specifications the measured tree holds.</summary>
    [Params(1, 10, 100, 1000, 10000)]
    public int Specifications { get; set; }

    /// <summary>
    /// Generates this size's tree, resolves the runner <see cref="SpechtEngineServiceCollectionExtensions.AddSpechtEngine"/>
    /// registers from the one provider <see cref="TypeRegistrar.Build"/> builds (ADR-0003), and refuses a tree the check
    /// does not count whole and clean.
    /// </summary>
    [GlobalSetup]
    public void GenerateTree()
    {
        _root = Directory.CreateTempSubdirectory("specht-check-").FullName;
        _resolver = new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();
        var generator = (SpechtTreeGenerator)_resolver.Resolve(typeof(SpechtTreeGenerator))!;
        var store = (SpechtTreeStore)_resolver.Resolve(typeof(SpechtTreeStore))!;
        store.Save(generator.Generate(Specifications), _root);
        _runner = (SpechtRunner?)_resolver.Resolve(typeof(SpechtRunner))
                  ?? throw new InvalidOperationException("AddSpechtEngine() registers no SpechtRunner.");

        var report = _runner.Run(_root);
        if (report.SpecificationCount == Specifications && report.Violations.Count == 0)
        {
            return;
        }

        DeleteTree();
        throw new InvalidOperationException(
            $"The check counted {report.SpecificationCount} of {Specifications} specifications and reported {report.Violations.Count} violations.");
    }

    /// <summary>Disposes the provider the runner came from and deletes this size's tree.</summary>
    [GlobalCleanup]
    public void DeleteTree()
    {
        (_resolver as IDisposable)?.Dispose();
        Directory.Delete(_root, recursive: true);
    }

    /// <summary>Runs the whole check from the tree's root.</summary>
    /// <returns>The report.</returns>
    [Benchmark]
    [WorkloadIdentity("check", "1", "generated-specification-tree", "1", "in-process")]
    public SpechtReport Check() => _runner.Run(_root);

    private string _root = string.Empty;
    private ITypeResolver? _resolver;
    private SpechtRunner _runner = null!;
}
