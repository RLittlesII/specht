namespace Specht.Model;

/// <summary>A finding the Gherkin claim reader makes at a one-based line of the file it reads.</summary>
/// <implements epic="0201" feature="F7" claim="B-015" />
internal sealed record GherkinClaimTextFinding(string DiagnosticId, int Line, string Text);
