using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Specht.Analysis.Bindings;

/// <summary>The effective identity, marker and lifecycle policy projected from validated pinned inputs.</summary>
public sealed class BindingPolicy
{
    public BindingPolicy(
        string schemaVersion,
        IReadOnlyDictionary<string, string> identifiers,
        IReadOnlyDictionary<string, string> markers,
        bool declaresEpicGrammar,
        string claimsSection,
        LifecycleOmissions omissions)
    {
        SchemaVersion = schemaVersion;
        Identifiers = new ReadOnlyDictionary<string, string>(identifiers.ToDictionary(
            static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal));
        Markers = new ReadOnlyDictionary<string, string>(markers.ToDictionary(
            static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal));
        DeclaresEpicGrammar = declaresEpicGrammar;
        ClaimsSection = claimsSection;
        Omissions = omissions;
    }

    public string SchemaVersion { get; }

    public IReadOnlyDictionary<string, string> Identifiers { get; }

    public IReadOnlyDictionary<string, string> Markers { get; }

    public bool DeclaresEpicGrammar { get; }

    public string ClaimsSection { get; }

    public LifecycleOmissions Omissions { get; }
}
