namespace Specht.Comparison;

/// <summary>The overall advisory judgment of a release comparison.</summary>
public enum ComparisonJudgment
{
    ReviewRequired,
    Incomplete,
    NoRegressionFlagged,
    NoBaseline,
}
