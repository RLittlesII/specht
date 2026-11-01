namespace Specht.Comparison;

/// <summary>The advisory judgment of one paired metric.</summary>
public enum MetricJudgment
{
    NoRegressionFlagged,
    ReviewRequired,
    InsufficientEvidence,
    NotComparable,
    Informational,
}
