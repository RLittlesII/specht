using System.Collections.Immutable;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Evidence;

[AutoFixture(typeof(SessionSide))]
internal sealed partial class SessionSideFixture
{
    public SessionSideFixture() => WithProductCommit(new string('a', 40)).WithHarnessVersion("native-v1").WithExporterVersion("0.15.8")
        .WithInvocation(["--filter", "*Check*"]).WithConditions(new MeasurementConditionsFixture()).WithInventoryKnown(true).WithExpectedCases([]);
}
