namespace Specht.Analysis.TestEvidence;

public sealed record TestEvidenceRunnerCaseRecord(
    string? CaseId,
    string? MethodSymbolId,
    string? RowId,
    string? DisplayName,
    TestEvidenceRunnerMethodRecord? Method = null);
