using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceMethod(
    string SymbolId,
    string DeclaringType,
    string Name,
    int GenericArity,
    IReadOnlyList<string> ParameterTypes,
    TestEvidenceLocation? Location);
