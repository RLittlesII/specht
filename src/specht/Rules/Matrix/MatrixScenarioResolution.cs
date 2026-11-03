namespace Specht.Rules.Matrix;

/// <summary>A Scenario cell title's resolution and the scenario it names, without a rule verdict.</summary>
/// <param name="Title">One title of the cell, trimmed.</param>
/// <param name="Status">Whether a scenario with the title carries the row's claim tag.</param>
/// <param name="File">The root-relative companion holding the named scenario, for a resolved or untagged title.</param>
/// <param name="Line">The named scenario's one-based keyword line, for a resolved or untagged title.</param>
/// <implements epic="0001" feature="F12" claim="B-008" />
public sealed record MatrixScenarioResolution(string Title, MatrixScenarioStatus Status, string? File = null, int? Line = null);
