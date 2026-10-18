namespace Specht.Analysis.Documentation;

/// <summary>An earlier published diagnostic identity, retained after retirement.</summary>
public sealed record DiagnosticPublication(string Id, string Finding, bool Retired = false);
