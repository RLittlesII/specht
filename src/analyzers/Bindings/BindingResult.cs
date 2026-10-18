using System;
using System.Collections.Generic;
using System.Linq;

namespace Specht.Analysis.Bindings;

/// <summary>A resolution finding with lifecycle eligibility only for a unique resolved target.</summary>
public sealed class BindingResult
{
    public BindingResult(
        BindingResolution status, BindingTarget? target, bool? isEligible, IReadOnlyList<BindingTarget> matches)
    {
        Status = status;
        Target = target;
        IsEligible = isEligible;
        Matches = Array.AsReadOnly(matches.ToArray());
    }

    public BindingResolution Status { get; }

    public BindingTarget? Target { get; }

    public bool? IsEligible { get; }

    public IReadOnlyList<BindingTarget> Matches { get; }
}
