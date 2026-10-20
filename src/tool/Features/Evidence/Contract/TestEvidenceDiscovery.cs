using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

public sealed record TestEvidenceDiscovery(
    TestEvidenceDiscoveryState State,
    string? SessionId,
    IReadOnlyList<TestEvidenceMethod> Methods,
    IReadOnlyList<TestEvidenceCase> Cases);
