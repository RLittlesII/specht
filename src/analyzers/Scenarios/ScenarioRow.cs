namespace Specht.Analysis.Scenarios;

/// <summary>One producer-supplied outline row, or the single implicit row of a scenario, with its planned flag.</summary>
/// <implements epic="0201" feature="F4" claim="B-009" />
public sealed record ScenarioRow(string? RowId, string? CaseId, bool Planned);
