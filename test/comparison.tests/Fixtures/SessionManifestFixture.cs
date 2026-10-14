using System.Collections.Immutable;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Evidence;

[AutoFixture(typeof(SessionManifest))]
internal sealed partial class SessionManifestFixture
{
    public SessionManifestFixture() => WithFormatVersion(1).WithPairDigest(new string('b', 64)).WithComparatorCommit(new string('c', 40))
        .WithComparatorVersion("1").WithRunId("run-1").WithAttempt(1).WithSessionId("session-1").WithRunnerIdentity("runner-1")
        .WithPrevious(new SessionSideFixture()).WithCurrent(new SessionSideFixture());
}
