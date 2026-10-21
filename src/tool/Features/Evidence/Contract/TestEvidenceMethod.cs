using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceMethod(
    string SymbolId,
    string DeclaringType,
    string Name,
    int GenericArity,
    IReadOnlyList<string> ParameterTypes,
    TestEvidenceLocation? Location);
