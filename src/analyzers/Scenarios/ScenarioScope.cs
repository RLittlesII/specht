namespace Specht.Analysis.Scenarios;

/// <summary>A scenario's claim-owning scope, whole: a Feature-level default or an explicit replacement.</summary>
/// <implements epic="0201" feature="F4" claim="B-006" />
/// <implements epic="0201" feature="F4" claim="B-007" />
public sealed record ScenarioScope(string? Epic, string? Feature);
