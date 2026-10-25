namespace Specht.Acceptance.Reconciliation;

internal sealed record CaseSpec(string CaseId, bool Owned, bool Selected, string? Outcome, string? Link = null);
