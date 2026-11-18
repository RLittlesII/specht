using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(Observation))]
internal sealed partial class ObservationFixture
{
    public ObservationFixture() => WithMetric(MetricKind.Time).WithValue(100).WithUnit("ns").WithObserver("benchmark-process")
        .WithInterval(new TimingIntervalFixture()).WithInvalidReason(null).WithSourceFile("current/full.json").WithLocator("/Benchmarks/0/Statistics");
}
