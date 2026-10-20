using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

public sealed record TestEvidenceResults(string State, string? RunId, IReadOnlyList<TestEvidenceOutcome> Cases);
