using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Resources;
using Microsoft.CodeAnalysis;

namespace Specht.Analysis.Documentation;

/// <summary>The append-only SPCT XML diagnostic registry, independent of the checker vocabulary.</summary>
public sealed class DocumentationDiagnostics
{
    public DocumentationDiagnostics(IReadOnlyList<DiagnosticPublication>? previous = null)
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
            .Select(static entry => Descriptor(entry.Id, entry.Finding, entry.Finding == "Duplicate" ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning))
            .ToImmutableArray();
    }

    public IReadOnlyList<DiagnosticPublication> Publications { get; } = Array.AsReadOnly(new[]
    {
        new DiagnosticPublication("SPCT0002", "Shape"), new DiagnosticPublication("SPCT0003", "Grammar"),
        new DiagnosticPublication("SPCT0004", "Absent"), new DiagnosticPublication("SPCT0005", "Ambiguous"),
        new DiagnosticPublication("SPCT0006", "Ineligible"), new DiagnosticPublication("SPCT0007", "Required"),
        new DiagnosticPublication("SPCT0008", "Defaults"), new DiagnosticPublication("SPCT0009", "Duplicate"),
    });

    public ImmutableArray<DiagnosticDescriptor> Descriptors { get; }

    private static DiagnosticDescriptor Descriptor(string id, string key, DiagnosticSeverity severity = DiagnosticSeverity.Warning) => new(
        id, new LocalizableResourceString(key, Resources, typeof(DocumentationDiagnostics)),
        new LocalizableResourceString("Message", Resources, typeof(DocumentationDiagnostics)), "Traceability", severity, true,
        description: new LocalizableResourceString(key, Resources, typeof(DocumentationDiagnostics)),
        helpLinkUri: "https://github.com/RLittlesII/specht/blob/main/src/codefixes/.spec/README.md");

    private static readonly ResourceManager Resources = new("Specht.Analysis.Documentation.DocumentationResources", typeof(DocumentationDiagnostics).Assembly);
}
