namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceAnalysisInputs(
    string ProjectPath,
    string TargetFramework,
    string SourceFingerprint,
    string AssemblySha256,
    string ConfigurationFingerprint,
    string PolicyFingerprint);
