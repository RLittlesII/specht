namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceReadResult(bool IsValid, string? Failure, bool IsCurrent, TestEvidenceArtifact? Artifact)
{
    internal static TestEvidenceReadResult Valid(TestEvidenceArtifact artifact, bool isCurrent) => new(true, null, isCurrent, artifact);
    internal static TestEvidenceReadResult Invalid() => new(false, "invalidContract", false, null);
    internal static TestEvidenceReadResult Unsupported() => new(false, "unsupportedContract", false, null);
}
