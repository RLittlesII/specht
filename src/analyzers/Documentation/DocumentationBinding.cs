using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Documentation;

/// <summary>A reserved element retaining raw identity and independent shape and resolution verdicts.</summary>
public sealed record DocumentationBinding(
    string SymbolId, string Role, IReadOnlyDictionary<string, string> Attributes,
    Location Location, bool ShapeValid, BindingResult? Resolution)
{
    public bool IsEvidence => ShapeValid && Role != "spec" && Resolution?.Status == BindingResolution.Resolved && Resolution.IsEligible == true;
}
