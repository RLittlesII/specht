using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceResults(string State, string? RunId, IReadOnlyList<TestEvidenceOutcome> Cases);
