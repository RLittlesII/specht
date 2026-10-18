using System;
using System.Collections.Generic;
using System.Linq;

namespace Specht.Analysis.Bindings;

/// <summary>A Feature declaration and its complete claim inventory.</summary>
public sealed class BindingFeature
{
    public BindingFeature(FeatureIdentity identity, string? state, IReadOnlyList<BindingClaim> claims, string relativePath)
    {
        Identity = identity;
        State = state;
        Claims = Array.AsReadOnly(claims.ToArray());
        RelativePath = relativePath;
    }

    public FeatureIdentity Identity { get; }

    public string? State { get; }

    public IReadOnlyList<BindingClaim> Claims { get; }

    public string RelativePath { get; }
}
