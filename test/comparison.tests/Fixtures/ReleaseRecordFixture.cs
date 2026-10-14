using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Selection;

[AutoFixture(typeof(ReleaseRecord))]
internal sealed partial class ReleaseRecordFixture
{
    public ReleaseRecordFixture() => WithRepository("example/tool").WithId(1).WithTag("v1")
        .WithPublishedAt(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        .WithCommit(new string('a', 40)).WithResolutionFailure(null);
}
