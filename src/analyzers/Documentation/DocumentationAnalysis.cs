using System.Collections.Generic;

namespace Specht.Analysis.Documentation;

/// <summary>Raw elements, unique eligible relationships and findings for one logical declaration.</summary>
public sealed record DocumentationAnalysis(
    IReadOnlyList<DocumentationBinding> Elements,
    IReadOnlyList<DocumentationBinding> Relationships,
    IReadOnlyList<DocumentationFinding> Findings);
