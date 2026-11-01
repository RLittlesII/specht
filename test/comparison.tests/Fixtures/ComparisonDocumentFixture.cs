using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Reporting;

[AutoFixture(typeof(ComparisonDocument))]
internal sealed partial class ComparisonDocumentFixture
{
    public ComparisonDocumentFixture()
        : this(PreviousExport, CurrentExport)
    {
    }

    public ComparisonDocumentFixture(string previousExport, string currentExport)
    {
        WorkloadCase small = new WorkloadCaseFixture().WithOperationRevision("operation-revision-2").WithFixtureRevision("fixture-revision-3");
        var large = small with { Parameters = small.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) };
        SessionManifest session = new SessionManifestFixture().WithPairDigest(ComparisonJson.Digest((ReleasePair)ReportFixtures.Pair()));
        MeasurementInventory previous = new MeasurementInventoryFixture().WithCases([new CaseEvidenceFixture().WithCase(small).WithObservations([
            new ObservationFixture().WithSourceFile(previousExport),
            new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null)
                .WithSourceFile(previousExport).WithLocator("/Benchmarks/0/Memory")])]);
        MeasurementInventory current = new MeasurementInventoryFixture().WithCases([
            new CaseEvidenceFixture().WithCase(small).WithObservations([
                new ObservationFixture().WithValue(120).WithInterval(new TimingIntervalFixture().WithLower(118).WithUpper(122))
                    .WithSourceFile(currentExport),
                new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(32).WithUnit("B").WithInterval(null)
                    .WithSourceFile(currentExport).WithLocator("/Benchmarks/0/Memory")]),
            new CaseEvidenceFixture().WithCase(large).WithObservations([
                new ObservationFixture().WithSourceFile(currentExport).WithLocator("/Benchmarks/1/Statistics"),
                new ObservationFixture().WithMetric(MetricKind.ManagedAllocation).WithValue(64).WithUnit("B").WithInterval(null)
                    .WithSourceFile(currentExport).WithLocator("/Benchmarks/1/Memory")])]);
        var document = ComparisonEvaluation.Evaluate(session, CasePairing.Pair(previous, current, session));
        WithFormatVersion(document.FormatVersion).WithSession(session).WithPolicy(document.Policy).WithCases(document.Cases)
            .WithJudgment(document.Judgment).WithGaps(document.Gaps);
    }

    public const string PreviousExport = ".artifacts/benchmarks/previous/Synthetic-report-full.json";
    public const string CurrentExport = ".artifacts/benchmarks/current/Synthetic-report-full.json";
}
