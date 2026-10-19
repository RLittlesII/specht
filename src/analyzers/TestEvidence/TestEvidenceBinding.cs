using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceBinding(
    string SymbolId,
    string Role,
    IReadOnlyDictionary<string, string> Attributes,
    bool ShapeValid,
    string? Status,
    bool? IsEligible,
    TestEvidenceTarget? Target,
    IReadOnlyList<TestEvidenceTarget> Matches,
    TestEvidenceLocation? Location);
