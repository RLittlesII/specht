using System;
using System.Collections.Generic;
using System.Linq;

namespace Specht.Analysis.Bindings;

/// <summary>An immutable declaration inventory that retains duplicate records.</summary>
public sealed class BindingCatalog
{
    public BindingCatalog(IReadOnlyList<BindingFeature> features, IReadOnlyList<BindingAdr> adrs)
        : this(features, adrs, Array.Empty<BindingFinding>())
    {
    }

    public BindingCatalog(IReadOnlyList<BindingFeature> features, IReadOnlyList<BindingAdr> adrs, IReadOnlyList<BindingFinding> findings)
    {
        Findings = Array.AsReadOnly(findings.ToArray());
        Features = Array.AsReadOnly(features.ToArray());
        Adrs = Array.AsReadOnly(adrs.ToArray());
        var guard = new BindingProvenanceGuard();
        foreach (var path in Features.Select(static feature => feature.RelativePath).Concat(Adrs.Select(static adr => adr.RelativePath)))
        {
            guard.Validate(path);
        }
    }

    public IReadOnlyList<BindingFeature> Features { get; }

    public IReadOnlyList<BindingAdr> Adrs { get; }

    public IReadOnlyList<BindingFinding> Findings { get; }
}
