namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceRunnerCollectionResult(
    bool IsValid,
    string AdapterId,
    string AdapterVersion,
    TestEvidenceDiscovery Discovery,
    TestEvidenceResults Results);
