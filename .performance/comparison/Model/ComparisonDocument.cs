using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable version-1 comparison of a release pair's evaluated cases.</summary>
public sealed record ComparisonDocument(
    int FormatVersion,
    SessionManifest Session,
    ComparisonPolicy Policy,
    ImmutableArray<CaseComparison> Cases,
    ComparisonJudgment Judgment,
    ImmutableArray<CoverageGap> Gaps);
