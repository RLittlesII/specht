namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceReadResult(bool IsValid, string? Failure, bool IsCurrent, TestEvidenceArtifact? Artifact)
{
    /// <implements epic="0201" feature="F3" claim="B-009" />
    internal static TestEvidenceReadResult Valid(TestEvidenceArtifact artifact, bool isCurrent) => new(true, null, isCurrent, artifact);
    /// <implements epic="0201" feature="F3" claim="B-009" />
    internal static TestEvidenceReadResult Invalid() => new(false, "invalidContract", false, null);
    /// <implements epic="0201" feature="F3" claim="B-009" />
    internal static TestEvidenceReadResult Unsupported() => new(false, "unsupportedContract", false, null);
}
