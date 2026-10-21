namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceAnalysisInputs(
    string ProjectPath,
    string TargetFramework,
    string SourceFingerprint,
    string AssemblySha256,
    string ConfigurationFingerprint,
    string PolicyFingerprint);
