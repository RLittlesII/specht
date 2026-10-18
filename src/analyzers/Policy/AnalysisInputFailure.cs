namespace Specht.Analysis.Policy;

/// <summary>An unavailable-analysis finding independent of its diagnostic presentation.</summary>
public sealed record AnalysisInputFailure(string RelativePath, string Message);
