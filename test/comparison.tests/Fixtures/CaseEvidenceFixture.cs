using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(CaseEvidence))]
internal sealed partial class CaseEvidenceFixture
{
    public CaseEvidenceFixture() => WithCase(new WorkloadCaseFixture()).WithIdentityKnown(true).WithFailure(null)
        .WithObservations([new ObservationFixture(), new ObservationFixture().WithMetric(MetricKind.ManagedAllocation)
            .WithValue(32).WithUnit("B").WithInterval(null).WithLocator("/Benchmarks/0/Memory")]);
}
