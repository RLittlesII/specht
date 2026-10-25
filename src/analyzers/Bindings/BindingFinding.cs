namespace Specht.Analysis.Bindings;

public sealed record BindingFinding(string DiagnosticId, string RelativePath, int Line, string Text);
