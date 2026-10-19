using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceCaseCorrelationResult(bool IsCurrent, IReadOnlyList<TestEvidenceCaseMatch> Matches);
