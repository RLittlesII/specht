using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceCase(string CaseId, string MethodSymbolId, string? RowId, string? DisplayName,
    IReadOnlyList<string>? Categories = null, bool? Owned = null, bool? Selected = null);
