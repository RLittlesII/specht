using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
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
