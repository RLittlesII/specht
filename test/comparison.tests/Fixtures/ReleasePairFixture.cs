using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Reporting;

[AutoFixture(typeof(ReleasePair))]
internal sealed partial class ReleasePairFixture
{
    public ReleasePairFixture() => WithFormatVersion(1)
        .WithCurrent(new ReleaseRecordFixture().WithId(20).WithTag("v2").WithPublishedAt(new DateTimeOffset(2026, 1, 4, 0, 0, 0, TimeSpan.Zero)))
        .WithPrevious(new ReleaseRecordFixture().WithId(10).WithTag("v1").WithCommit(new string('e', 40)))
        .WithSelectionPolicy("publication-order-v1").WithCatalogDigest(new string('f', 64)).WithSelectingRunId("run-1");
}
