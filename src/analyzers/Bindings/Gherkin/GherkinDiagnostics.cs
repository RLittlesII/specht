using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Specht.Analysis.Documentation;

namespace Specht.Analysis.Bindings.Gherkin;

public sealed class GherkinDiagnostics
{
    public GherkinDiagnostics(IReadOnlyList<DiagnosticPublication>? previous = null) => Previous = previous;

    public IReadOnlyList<DiagnosticPublication>? Previous { get; }

    public IReadOnlyList<DiagnosticPublication> Publications { get; } = Array.Empty<DiagnosticPublication>();

    public ImmutableArray<DiagnosticDescriptor> Descriptors { get; } = ImmutableArray<DiagnosticDescriptor>.Empty;
}
