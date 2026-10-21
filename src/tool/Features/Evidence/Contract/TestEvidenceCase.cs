namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed record TestEvidenceCase(string CaseId, string MethodSymbolId, string? RowId, string? DisplayName);
