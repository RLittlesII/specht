using System.Collections.Immutable;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Evaluation;

[Trait("Tier", "Unit")]
public sealed class ComparisonEvaluationUnitTests
{
    [Fact]
    public void CompatibleTimeScales_WhenEvaluating_ShouldShareNanosecondsWithIntervalsScaledWithTheMean()
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(1).WithUnit("ms").WithInterval(new TimingIntervalFixture().WithLower(0.98m).WithUpper(1.02m)),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(1000000).WithUnit("ns").WithInterval(new TimingIntervalFixture().WithLower(980000).WithUpper(1020000)),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var timing = document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.Time);
        timing.Unit.Should().Be("ns");
        timing.Previous.Should().Be(1000000m);
        timing.Current.Should().Be(1000000m);
        timing.AbsoluteChange.Should().Be(0m);
        timing.PercentChange.Should().Be(0m);
        timing.PreviousInterval.Should().Be(new TimingInterval(980000m, 1020000m, 0.999m));
        timing.CurrentInterval.Should().Be(new TimingInterval(980000m, 1020000m, 0.999m));
        timing.Judgment.Should().Be(MetricJudgment.NoRegressionFlagged);
    }

    [Theory]
    [ClassData(typeof(ByteScaleRows))]
    public void CompatibleByteScales_WhenEvaluating_ShouldShareBytesWithZeroChange(
        decimal previousValue, string previousUnit, decimal currentValue, string currentUnit, decimal bytes)
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(previousValue).WithUnit(previousUnit).WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(currentValue).WithUnit(currentUnit).WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var allocation = document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation);
        allocation.Unit.Should().Be("B");
        allocation.Previous.Should().Be(bytes);
        allocation.Current.Should().Be(bytes);
        allocation.AbsoluteChange.Should().Be(0m);
        allocation.PercentChange.Should().Be(0m);
    }

    [Theory]
    [ClassData(typeof(ChangeRows))]
    public void ComparableValues_WhenEvaluating_ShouldReportUnroundedAbsoluteAndPercentChange(MetricKind metric, string unit,
        decimal previousValue, decimal currentValue, string sharedUnit, decimal normalisedPrevious, decimal normalisedCurrent,
        decimal absoluteChange, decimal percentChange)
    {
        // Given
        Observation timing = new ObservationFixture();
        Observation allocation = new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null);
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations(
            metric == MetricKind.Time ? [timing with { Value = previousValue, Unit = unit }, allocation]
                : [timing, allocation with { Value = previousValue, Unit = unit }])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations(
            metric == MetricKind.Time ? [timing with { Value = currentValue, Unit = unit }, allocation]
                : [timing, allocation with { Value = currentValue, Unit = unit }])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var result = document.Cases.Single().Metrics.Single(m => m.Metric == metric);
        result.Unit.Should().Be(sharedUnit);
        result.Previous.Should().Be(normalisedPrevious);
        result.Current.Should().Be(normalisedCurrent);
        result.AbsoluteChange.Should().Be(absoluteChange);
        result.PercentChange.Should().Be(percentChange);
    }

    [Theory]
    [ClassData(typeof(FlaggedTimingRows))]
    public void TimingRisingMoreThanTenPercentWithSeparatedIntervals_WhenEvaluating_ShouldRequireReviewWithReason(
        decimal currentMean, decimal currentHalfWidth, decimal percentChange)
    {
        // Given
        Observation allocation = new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null);
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(100).WithUnit("ms").WithInterval(new TimingIntervalFixture().WithLower(98).WithUpper(102)),
            allocation])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(currentMean).WithUnit("ms").WithInterval(new TimingIntervalFixture()
                .WithLower(currentMean - currentHalfWidth).WithUpper(currentMean + currentHalfWidth)),
            allocation])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var timing = document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.Time);
        timing.Judgment.Should().Be(MetricJudgment.ReviewRequired);
        timing.PercentChange.Should().Be(percentChange);
        timing.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [ClassData(typeof(UnflaggedTimingRows))]
    public void TimingWithoutStrictIncreaseOrSeparatedIntervals_WhenEvaluating_ShouldNotFlag(decimal currentMean, decimal currentHalfWidth)
    {
        // Given
        Observation allocation = new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null);
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(100).WithUnit("ms").WithInterval(new TimingIntervalFixture().WithLower(98).WithUpper(102)),
            allocation])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(currentMean).WithUnit("ms").WithInterval(new TimingIntervalFixture()
                .WithLower(currentMean - currentHalfWidth).WithUpper(currentMean + currentHalfWidth)),
            allocation])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.Time).Judgment.Should().Be(MetricJudgment.NoRegressionFlagged);
        document.Judgment.Should().Be(ComparisonJudgment.NoRegressionFlagged);
    }

    [Fact]
    public void AllocationRisingMoreThanTenPercent_WhenEvaluating_ShouldRequireReviewWithReason()
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1000).WithUnit("B").WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1101).WithUnit("B").WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var allocation = document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation);
        allocation.Judgment.Should().Be(MetricJudgment.ReviewRequired);
        allocation.PercentChange.Should().Be(10.1m);
        allocation.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Theory]
    [ClassData(typeof(UnflaggedAllocationRows))]
    public void AllocationNotRisingMoreThanTenPercent_WhenEvaluating_ShouldNotFlag(decimal currentBytes)
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1000).WithUnit("B").WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(currentBytes).WithUnit("B").WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation).Judgment
            .Should().Be(MetricJudgment.NoRegressionFlagged);
        document.Judgment.Should().Be(ComparisonJudgment.NoRegressionFlagged);
    }

    [Fact]
    public void AllocationRiseWithTimingImprovement_WhenEvaluating_ShouldFlagTheAllocationIndependently()
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(100).WithUnit("ms").WithInterval(new TimingIntervalFixture().WithLower(98).WithUpper(102)),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1000).WithUnit("B").WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(80).WithUnit("ms").WithInterval(new TimingIntervalFixture().WithLower(78).WithUpper(82)),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1200).WithUnit("B").WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var metrics = document.Cases.Single().Metrics;
        metrics.Single(static m => m.Metric == MetricKind.Time).Judgment.Should().Be(MetricJudgment.NoRegressionFlagged);
        metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation).Judgment.Should().Be(MetricJudgment.ReviewRequired);
        document.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
    }

    [Theory]
    [ClassData(typeof(ZeroAllocationRows))]
    public void ZeroPreviousAllocation_WhenEvaluating_ShouldLeaveThePercentageNotDefined(decimal currentBytes, MetricJudgment judgment,
        ComparisonJudgment overall)
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(0).WithUnit("B").WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(currentBytes).WithUnit("B").WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var allocation = document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation);
        allocation.Judgment.Should().Be(judgment);
        allocation.PercentChange.Should().BeNull();
        allocation.Previous.Should().Be(0m);
        allocation.Current.Should().Be(currentBytes);
        allocation.AbsoluteChange.Should().Be(currentBytes);
        document.Judgment.Should().Be(overall);
    }

    [Theory]
    [ClassData(typeof(InvalidTimingRows))]
    public void InvalidTimingEvidence_WhenEvaluating_ShouldBeInsufficientWhileTheAllocationStillFlags(string unit, decimal mean, decimal lower,
        decimal upper, decimal confidenceLevel)
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(mean).WithUnit(unit).WithInterval(new TimingIntervalFixture().WithLower(lower).WithUpper(upper)
                .WithConfidenceLevel(confidenceLevel)),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1000).WithUnit("B").WithInterval(null)])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(100).WithUnit("ms").WithInterval(new TimingIntervalFixture().WithLower(98).WithUpper(102)),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1200).WithUnit("B").WithInterval(null)])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var comparison = document.Cases.Single();
        var timing = comparison.Metrics.Single(static m => m.Metric == MetricKind.Time);
        timing.Judgment.Should().Be(MetricJudgment.InsufficientEvidence);
        timing.Reason.Should().NotBeNullOrWhiteSpace();
        comparison.Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation).Judgment.Should().Be(MetricJudgment.ReviewRequired);
        document.Gaps.Should().ContainSingle().Which.Should().Match<CoverageGap>(gap =>
            gap.CaseKey == comparison.Key && gap.Metric == MetricKind.Time && !string.IsNullOrWhiteSpace(gap.Reason));
        document.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
    }

    [Theory]
    [ClassData(typeof(TimingRecordCountRows))]
    public void RequiredTimingMissingOrDuplicated_WhenEvaluating_ShouldKeepThePairingReasonAsAGap(int currentTimingRecords)
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture();
        Observation allocation = new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null);
        Observation timing = new ObservationFixture();
        var records = Enumerable.Range(0, currentTimingRecords).Select(index => timing with { Locator = $"/Benchmarks/{index}/Statistics" });
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture()
            .WithObservations([.. records, allocation])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var comparison = document.Cases.Single();
        var result = comparison.Metrics.Single(static m => m.Metric == MetricKind.Time);
        result.Judgment.Should().Be(MetricJudgment.InsufficientEvidence);
        result.Reason.Should().Be(pairs.Single().Metrics.Single(static m => m.Metric == MetricKind.Time).Reason);
        result.Evidence.Current.Should().HaveCount(currentTimingRecords);
        comparison.Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation).Judgment.Should().Be(MetricJudgment.NoRegressionFlagged);
        document.Gaps.Should().ContainSingle().Which.Should().Match<CoverageGap>(gap => gap.CaseKey == comparison.Key && gap.Metric == MetricKind.Time);
        document.Judgment.Should().Be(ComparisonJudgment.Incomplete);
    }

    [Fact]
    public void EmptyInventory_WhenEvaluating_ShouldBeIncomplete()
    {
        // Given
        MeasurementInventory empty = new MeasurementInventoryFixture().WithCases([]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(empty, empty, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        document.Cases.Should().BeEmpty();
        document.Judgment.Should().Be(ComparisonJudgment.Incomplete);
    }

    [Theory]
    [ClassData(typeof(OneSidedRows))]
    public void OneSidedCase_WhenEvaluating_ShouldReportACaseGapWithoutAJudgment(bool added, PairingStatus status)
    {
        // Given
        MeasurementInventory present = new MeasurementInventoryFixture();
        MeasurementInventory absent = new MeasurementInventoryFixture().WithCases([]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(added ? absent : present, added ? present : absent, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var comparison = document.Cases.Single();
        comparison.Status.Should().Be(status);
        comparison.Metrics.Should().BeEmpty();
        document.Gaps.Should().ContainSingle().Which.Should().Match<CoverageGap>(gap =>
            gap.CaseKey == comparison.Key && gap.Metric == null && !string.IsNullOrWhiteSpace(gap.Reason));
        document.Judgment.Should().Be(ComparisonJudgment.Incomplete);
    }

    [Fact]
    public void NotComparableCase_WhenEvaluating_ShouldReportEachRequiredMetricAsAGapWithItsReason()
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        SessionManifest session = new SessionManifestFixture();
        session = session with { Current = session.Current with { Conditions = session.Current.Conditions with { Runtime = "other" } } };
        var pairs = CasePairing.Pair(inventory, inventory, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var comparison = document.Cases.Single();
        comparison.Status.Should().Be(PairingStatus.NotComparable);
        comparison.Metrics.Should().HaveCount(2).And.OnlyContain(static m => m.Judgment == MetricJudgment.NotComparable);
        comparison.Metrics.Select(static m => m.Reason).Should().Equal(pairs.Single().Metrics.Select(static m => m.Reason));
        document.Gaps.Select(static gap => (gap.CaseKey, gap.Metric)).Should().BeEquivalentTo(
            new[] { (comparison.Key, (MetricKind?) MetricKind.Time), (comparison.Key, (MetricKind?) MetricKind.ManagedAllocation) });
        document.Judgment.Should().Be(ComparisonJudgment.Incomplete);
    }

    [Fact]
    public void FlagWithCoverageGaps_WhenEvaluating_ShouldRequireReviewAndRetainEveryGap()
    {
        // Given
        CaseEvidence common = new CaseEvidenceFixture().WithObservations([
            new ObservationFixture(),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(1000).WithUnit("B").WithInterval(null)]);
        Observation timing = common.Observations[0];
        var flagged = common with
        {
            Observations = [timing, timing with { Locator = "/Benchmarks/1/Statistics" }, common.Observations[1] with { Value = 1200 }],
        };
        var removed = common with { Case = common.Case with { OperationId = "init" } };
        var added = common with { Case = common.Case with { OperationId = "startup" } };
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([common, removed]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([flagged, added]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var flaggedKey = pairs.Single(static p => p.Status == PairingStatus.InsufficientEvidence).Key;
        document.Cases.Single(c => c.Key == flaggedKey).Metrics.Single(static m => m.Metric == MetricKind.ManagedAllocation).Judgment
            .Should().Be(MetricJudgment.ReviewRequired);
        document.Gaps.Select(static gap => (gap.CaseKey, gap.Metric)).Should().BeEquivalentTo(new[]
        {
            (flaggedKey, (MetricKind?)MetricKind.Time),
            (pairs.Single(static p => p.Status == PairingStatus.Added).Key, (MetricKind?)null),
            (pairs.Single(static p => p.Status == PairingStatus.Removed).Key, (MetricKind?)null),
        });
        document.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
    }

    [Fact]
    public void CompleteUnflaggedComparison_WhenEvaluating_ShouldReportNoRegressionFlagged()
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(inventory, inventory, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        document.Cases.Single().Metrics.Should().HaveCount(2).And.OnlyContain(static m => m.Judgment == MetricJudgment.NoRegressionFlagged);
        document.Gaps.Should().BeEmpty();
        document.Judgment.Should().Be(ComparisonJudgment.NoRegressionFlagged);
    }

    [Theory]
    [ClassData(typeof(PeakMemoryRows))]
    public void ToolPeakMemory_WhenEvaluating_ShouldStayInformationalWithoutAFlagOrGap(decimal? previousPeak)
    {
        // Given
        Observation peak = new ObservationFixture().WithMetric(MetricKind.ToolPeakMemory).WithValue(2000).WithUnit("B")
            .WithObserver("tool-process").WithInterval(null);
        MeasurementInventory previous = new MeasurementInventoryFixture();
        if (previousPeak is { } value)
        {
            previous = previous with { Cases = [previous.Cases[0] with { Observations = previous.Cases[0].Observations.Add(peak with { Value = value }) }] };
        }

        MeasurementInventory current = new MeasurementInventoryFixture();
        current = current with { Cases = [current.Cases[0] with { Observations = current.Cases[0].Observations.Add(peak) }] };
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.ToolPeakMemory).Judgment.Should().Be(MetricJudgment.Informational);
        document.Gaps.Should().BeEmpty();
        document.Judgment.Should().Be(ComparisonJudgment.NoRegressionFlagged);
    }

    [Theory]
    [ClassData(typeof(MixedScaleTimingRows))]
    public void TimingInDifferentScales_WhenEvaluating_ShouldScreenTheNormalisedIntervals(decimal previousMean, decimal previousHalfWidth,
        string previousUnit, decimal currentMean, decimal currentHalfWidth, string currentUnit, MetricJudgment judgment)
    {
        // Given
        Observation allocation = new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null);
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(previousMean).WithUnit(previousUnit).WithInterval(new TimingIntervalFixture()
                .WithLower(previousMean - previousHalfWidth).WithUpper(previousMean + previousHalfWidth)),
            allocation])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations([
            new ObservationFixture().WithValue(currentMean).WithUnit(currentUnit).WithInterval(new TimingIntervalFixture()
                .WithLower(currentMean - currentHalfWidth).WithUpper(currentMean + currentHalfWidth)),
            allocation])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        document.Cases.Single().Metrics.Single(static m => m.Metric == MetricKind.Time).Judgment.Should().Be(judgment);
    }

    [Theory]
    [ClassData(typeof(OneThirdIncreaseRows))]
    public void FlaggedMetric_WhenEvaluating_ShouldNotEmbedTheUnroundedPercentageInItsReason(MetricKind metric, string unit, decimal halfWidth)
    {
        // Given
        Observation timing = new ObservationFixture();
        Observation allocation = new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null);
        Observation changed = new ObservationFixture().WithMetric(metric).WithUnit(unit).WithInterval(metric == MetricKind.Time
            ? new TimingIntervalFixture().WithLower(300 - halfWidth).WithUpper(300 + halfWidth) : (TimingInterval?) null).WithValue(300);
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations(
            metric == MetricKind.Time ? [changed, allocation] : [timing, changed])]);
        changed = changed with
        {
            Value = 400,
            Interval = changed.Interval is null ? null : changed.Interval with { Lower = 400 - halfWidth, Upper = 400 + halfWidth },
        };
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithObservations(
            metric == MetricKind.Time ? [changed, allocation] : [timing, changed])]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var result = document.Cases.Single().Metrics.Single(m => m.Metric == metric);
        result.Judgment.Should().Be(MetricJudgment.ReviewRequired);
        result.PercentChange.Should().Be(100m * 100m / 300m);
        result.Reason.Should().NotBeNullOrWhiteSpace().And
            .NotContain(result.PercentChange!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Theory]
    [ClassData(typeof(NoRequiredRowCaseRows))]
    public void CaseWithoutATimeOrAllocationRow_WhenEvaluating_ShouldReportACaseGapAndBeIncomplete(string arrangement)
    {
        // Given
        WorkloadCase workload = new WorkloadCaseFixture().WithRequiredMetrics([]);
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([]);
        MeasurementInventory current = previous;
        SessionManifest session = new SessionManifestFixture();
        if (arrangement == "expected case with no required metrics missing from both exports")
        {
            session = session with
            {
                Previous = session.Previous with { ExpectedCases = [workload] },
                Current = session.Current with { ExpectedCases = [workload] },
            };
        }
        else
        {
            CaseEvidence peakOnly = new CaseEvidenceFixture().WithCase(workload).WithObservations([new ObservationFixture()
                .WithMetric(MetricKind.ToolPeakMemory).WithValue(2000).WithUnit("B").WithObserver("tool-process").WithInterval(null)]);
            previous = previous with { Cases = [peakOnly] };
            current = previous;
            session = session with { Current = session.Current with { Conditions = session.Current.Conditions with { Runtime = "other" } } };
        }

        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var comparison = document.Cases.Single();
        comparison.Metrics.Should().NotContain(static m => m.Metric == MetricKind.Time || m.Metric == MetricKind.ManagedAllocation);
        document.Gaps.Should().ContainSingle().Which.Should().Match<CoverageGap>(gap =>
            gap.CaseKey == comparison.Key && gap.Metric == null && !string.IsNullOrWhiteSpace(gap.Reason));
        document.Judgment.Should().Be(ComparisonJudgment.Incomplete);
    }

    [Theory]
    [ClassData(typeof(NoMetricPairingReasonRows))]
    public void NoMetricCaseWithAPairingReason_WhenEvaluating_ShouldCarryThatReasonAsACaseGap(bool inventoryKnown, bool identityKnown,
        PairingStatus status)
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture().WithKnown(inventoryKnown).WithCases([]);
        CaseEvidence evidence = new CaseEvidenceFixture();
        if (!identityKnown)
        {
            evidence = evidence with { IdentityKnown = false, Failure = "Audited workload identity is unavailable." };
        }

        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([evidence]);
        SessionManifest session = new SessionManifestFixture();
        var pairs = CasePairing.Pair(previous, current, session);

        // When
        var document = ComparisonEvaluation.Evaluate(session, pairs);

        // Then
        var paired = pairs.Single();
        paired.Status.Should().Be(status);
        paired.Metrics.Should().BeEmpty();
        document.Gaps.Should().ContainSingle().Which.Should().Be(new CoverageGap(paired.Key, null, paired.Reason!));
        document.Judgment.Should().Be(ComparisonJudgment.Incomplete);
    }

    public sealed class ByteScaleRows : TheoryData<decimal, string, decimal, string, decimal>
    {
        public ByteScaleRows()
        {
            Add(1m, "KiB", 1024m, "B", 1024m);
            Add(1m, "KB", 1000m, "B", 1000m);
            Add(1m, "MiB", 1024m, "KiB", 1048576m);
            Add(1m, "MB", 1000m, "KB", 1000000m);
        }
    }

    public sealed class ChangeRows : TheoryData<MetricKind, string, decimal, decimal, string, decimal, decimal, decimal, decimal>
    {
        public ChangeRows()
        {
            Add(MetricKind.Time, "ms", 100m, 100.0123m, "ns", 100000000m, 100012300m, 12300m, 0.0123m);
            Add(MetricKind.ManagedAllocation, "B", 800m, 801m, "B", 800m, 801m, 1m, 0.125m);
            Add(MetricKind.ManagedAllocation, "B", 1000m, 900m, "B", 1000m, 900m, -100m, -10m);
        }
    }

    public sealed class FlaggedTimingRows : TheoryData<decimal, decimal, decimal>
    {
        public FlaggedTimingRows()
        {
            Add(112m, 2m, 12m);
            Add(110.0001m, 1m, 10.0001m);
        }
    }

    public sealed class UnflaggedTimingRows : TheoryData<decimal, decimal>
    {
        public UnflaggedTimingRows()
        {
            Add(110m, 1m);
            Add(109.9999m, 1m);
            Add(111m, 10m);
            Add(111m, 9m);
            Add(80m, 2m);
        }
    }

    public sealed class UnflaggedAllocationRows : TheoryData<decimal>
    {
        public UnflaggedAllocationRows()
        {
            Add(1100m);
            Add(1000m);
            Add(900m);
        }
    }

    public sealed class ZeroAllocationRows : TheoryData<decimal, MetricJudgment, ComparisonJudgment>
    {
        public ZeroAllocationRows()
        {
            Add(0m, MetricJudgment.NoRegressionFlagged, ComparisonJudgment.NoRegressionFlagged);
            Add(64m, MetricJudgment.InsufficientEvidence, ComparisonJudgment.Incomplete);
        }
    }

    public sealed class InvalidTimingRows : TheoryData<string, decimal, decimal, decimal, decimal>
    {
        public InvalidTimingRows()
        {
            Add("ms", 0m, -2m, 2m, 0.999m);
            Add("ms", -100m, -102m, -98m, 0.999m);
            Add("ms", 100m, 102m, 98m, 0.999m);
            Add("ms", 100m, 98m, 102m, 0m);
            Add("s", 100000000000000000000m, 99999999999999999998m, 100000000000000000002m, 0.999m);
        }
    }

    public sealed class TimingRecordCountRows : TheoryData<int>
    {
        public TimingRecordCountRows()
        {
            Add(0);
            Add(2);
        }
    }

    public sealed class OneSidedRows : TheoryData<bool, PairingStatus>
    {
        public OneSidedRows()
        {
            Add(true, PairingStatus.Added);
            Add(false, PairingStatus.Removed);
        }
    }

    public sealed class PeakMemoryRows : TheoryData<decimal?>
    {
        public PeakMemoryRows()
        {
            Add(1000m);
            Add((decimal?) null);
        }
    }

    public sealed class MixedScaleTimingRows : TheoryData<decimal, decimal, string, decimal, decimal, string, MetricJudgment>
    {
        public MixedScaleTimingRows()
        {
            Add(100000000m, 2000000m, "ns", 112m, 2m, "ms", MetricJudgment.ReviewRequired);
            Add(100m, 2m, "ms", 111000000m, 9000000m, "ns", MetricJudgment.NoRegressionFlagged);
        }
    }

    public sealed class OneThirdIncreaseRows : TheoryData<MetricKind, string, decimal>
    {
        public OneThirdIncreaseRows()
        {
            Add(MetricKind.Time, "ms", 1m);
            Add(MetricKind.ManagedAllocation, "B", 0m);
        }
    }

    public sealed class NoRequiredRowCaseRows : TheoryData<string>
    {
        public NoRequiredRowCaseRows()
        {
            Add("expected case with no required metrics missing from both exports");
            Add("tool peak memory only with a different runtime");
        }
    }

    public sealed class NoMetricPairingReasonRows : TheoryData<bool, bool, PairingStatus>
    {
        public NoMetricPairingReasonRows()
        {
            Add(false, true, PairingStatus.InsufficientEvidence);
            Add(true, false, PairingStatus.NotComparable);
        }
    }
}
