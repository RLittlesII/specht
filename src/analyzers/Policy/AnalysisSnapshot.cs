using System;
using System.Collections.Generic;
using System.Linq;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Policy;

/// <summary>The immutable input verdict for one current compilation snapshot.</summary>
public sealed class AnalysisSnapshot
{
    public AnalysisSnapshot(string contentKey, AnalysisPolicy? policy, BindingCatalog? catalog, IReadOnlyList<AnalysisInputFailure> failures)
    {
        ContentKey = contentKey;
        Policy = policy;
        Catalog = catalog;
        Failures = Array.AsReadOnly(failures.ToArray());
    }

    public string ContentKey { get; }
    public AnalysisPolicy? Policy { get; }
    public BindingCatalog? Catalog { get; }
    public IReadOnlyList<AnalysisInputFailure> Failures { get; }
    public bool Available => Failures.Count == 0 && Policy is not null && Catalog is not null;
}
