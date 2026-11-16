using System.Collections.Immutable;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(WorkloadCase))]
internal sealed partial class WorkloadCaseFixture
{
    public WorkloadCaseFixture() => WithOperationId("check").WithOperationRevision("1").WithFixtureId("synthetic-tree").WithFixtureRevision("1")
        .WithParameters(ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("Specifications", new ParameterValue("number", "1")))
        .WithBoundary("engine-run").WithRequiredMetrics([MetricKind.Time, MetricKind.ManagedAllocation]);
}
