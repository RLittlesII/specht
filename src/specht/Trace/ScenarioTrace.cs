namespace Specht.Trace;

/// <summary>A companion scenario carrying a claim's tag (<c>0001-F13</c> B-003).</summary>
/// <param name="Title">The scenario's title.</param>
/// <param name="File">The root-relative companion holding it.</param>
/// <param name="Line">The one-based line of its keyword.</param>
/// <implements epic="0001" feature="F13" claim="B-003" />
public sealed record ScenarioTrace(string Title, string File, int Line);
