namespace Specht.Analysis.Policy;

/// <summary>One explicit compiler input, with root-relative provenance and current text.</summary>
public sealed record AnalysisInput(string Kind, string RelativePath, string? Text);
