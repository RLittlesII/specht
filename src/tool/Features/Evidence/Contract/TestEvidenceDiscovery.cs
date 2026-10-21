using System.Collections.Generic;

namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceDiscovery(
    TestEvidenceDiscoveryState State,
    string? SessionId,
    IReadOnlyList<TestEvidenceMethod> Methods,
    IReadOnlyList<TestEvidenceCase> Cases);
