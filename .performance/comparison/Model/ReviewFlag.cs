namespace Specht.Comparison;

/// <summary>One metric of one case that a comparison flagged for review, with the screen it met.</summary>
public sealed record ReviewFlag(string CaseKey, MetricKind Metric, string Reason);
