using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceRunnerMethodRecord(
    string DeclaringType,
    string Name,
    int GenericArity,
    IReadOnlyList<string> ParameterTypes);
