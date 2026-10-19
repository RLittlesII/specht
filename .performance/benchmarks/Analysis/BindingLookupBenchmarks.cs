using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Specht.Analysis.Bindings;

namespace Specht.Benchmarks.Analysis;

/// <summary>Measures explicit claim lookup with independently varied catalog and binding counts.</summary>
public class BindingLookupBenchmarks
{
    /// <summary>Gets or sets the number of distinct Feature records in the catalog.</summary>
    [Params(1, 40, 160)]
    public int Records { get; set; }

    /// <summary>Gets or sets the number of explicit bindings resolved per operation.</summary>
    [Params(1, 40)]
    public int Bindings { get; set; }

    /// <summary>Builds and validates the synthetic catalog outside the timed operation.</summary>
    [GlobalSetup]
    public void PrepareCatalog()
    {
        var features = Enumerable.Range(0, Records).Select(index =>
        {
            var identity = index.ToString("D4", CultureInfo.InvariantCulture);
            return new BindingFeature(new FeatureIdentity(null, "cap:feature" + identity), "draft",
                [new BindingClaim("need/7", "Active")], $"src/feature{identity}/.spec/README.md");
        }).ToArray();
        var policy = new BindingPolicy("0.1.0",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["feature"] = "^cap:feature[0-9]+$",
                ["claim"] = "^need/[0-9]+$",
            },
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["approved"] = "approved",
                ["withdrawn"] = "Withdrawn",
                ["superseded"] = "Superseded",
            }, false, "3. Acceptance Criteria", new LifecycleOmissions(false, false, false));
        _resolver = new BindingResolver(new BindingCatalog(features, []), policy);
        _references = Enumerable.Range(0, Bindings).Select(index =>
            new ClaimReference(null, "cap:feature" + (index % Records).ToString("D4", CultureInfo.InvariantCulture), "need/7"))
            .ToArray();

        if (_references.Any(reference => _resolver.Resolve(reference).Status != BindingResolution.Resolved))
        {
            throw new InvalidOperationException("A synthetic claim did not resolve.");
        }
    }

    /// <summary>Resolves every explicit binding against one immutable catalog.</summary>
    /// <returns>The number of resolving references.</returns>
    [Benchmark]
    public int ResolveBindings()
    {
        var resolved = 0;
        foreach (var reference in _references)
        {
            if (_resolver.Resolve(reference).Status == BindingResolution.Resolved)
            {
                resolved++;
            }
        }

        return resolved;
    }

    private BindingResolver _resolver = null!;
    private ClaimReference[] _references = null!;
}
