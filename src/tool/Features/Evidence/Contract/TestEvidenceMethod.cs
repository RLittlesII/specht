using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

public sealed record TestEvidenceMethod(
    string SymbolId,
    string DeclaringType,
    string Name,
    int GenericArity,
    IReadOnlyList<string> ParameterTypes,
    TestEvidenceLocation? Location);
