using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(RunContext))]
internal sealed partial class RunContextFixture
{
    public RunContextFixture() => WithEventName("push").WithRef("refs/tags/v2").WithRepository("example/tool").WithRunId("1234").WithAttempt(2)
        .WithSha(new string('a', 40));
}
