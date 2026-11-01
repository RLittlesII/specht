using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Collection;

[AutoFixture(typeof(CollectionPlan))]
internal sealed partial class CollectionPlanFixture
{
    public CollectionPlanFixture() => WithPairDigest(new string('b', 64)).WithPreviousCommit(new string('1', 40))
        .WithCurrentCommit(new string('2', 40)).WithComparatorCommit(new string('c', 40)).WithComparatorVersion("1").WithRunId("run-1")
        .WithAttempt(1).WithSessionId("session-1").WithRunnerIdentity("runner-1").WithFilter("Specht.Benchmarks.*").WithJob("Short");
}
