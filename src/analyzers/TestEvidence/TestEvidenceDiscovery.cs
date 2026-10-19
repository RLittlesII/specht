using System.Collections.Generic;

namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceDiscovery(
    TestEvidenceDiscoveryState State,
    string? SessionId,
    IReadOnlyList<TestEvidenceMethod> Methods,
    IReadOnlyList<TestEvidenceCase> Cases);
