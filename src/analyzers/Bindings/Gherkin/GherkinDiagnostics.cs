using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Specht.Analysis.Documentation;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>The append-only registry of the native claim catalogue diagnostics, apart from the XML registry.</summary>
public sealed class GherkinDiagnostics
{
    public GherkinDiagnostics(IReadOnlyList<DiagnosticPublication>? previous = null)
    {
        foreach (var published in previous ?? Array.Empty<DiagnosticPublication>())
        {
            var current = Publications.SingleOrDefault(entry => entry.Id == published.Id);
            if (current is null || current.Finding != published.Finding || published.Retired && !current.Retired)
            {
                throw new ArgumentException("A published diagnostic identity cannot be removed, reassigned or revived.", nameof(previous));
            }
        }

        Descriptors = Publications.Where(static entry => !entry.Retired)
            .Select(entry => Descriptor(entry.Id, Title(entry.Finding))).ToImmutableArray();
    }

    public IReadOnlyList<DiagnosticPublication> Publications { get; } = Array.AsReadOnly(new[]
    {
        new DiagnosticPublication("SPCT0010", "Duplicate"), new DiagnosticPublication("SPCT0011", "Placement"),
        new DiagnosticPublication("SPCT0012", "Missing"),
    });

    public ImmutableArray<DiagnosticDescriptor> Descriptors { get; }

    private static DiagnosticDescriptor Descriptor(string id, string title) => new(
        id, title, "{0}", "Traceability", DiagnosticSeverity.Warning, true, description: title,
        helpLinkUri: "https://github.com/RLittlesII/specht/blob/main/src/codeanalysis/docs/" + id + ".md");

    private static string Title(string finding) => finding switch
    {
        "Duplicate" => "Duplicate claim declaration",
        "Placement" => "Claim tag where no claim is declared",
        _ => "Scenario declares no claim",
    };
}
