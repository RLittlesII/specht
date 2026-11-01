using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Collection;

[AutoFixture(typeof(ReleasePair))]
internal sealed partial class ReleasePairFixture
{
    public ReleasePairFixture() => WithFormatVersion(1)
        .WithCurrent(new ReleaseRecordFixture().WithId(20).WithTag("v2").WithCommit(new string('2', 40)))
        .WithPrevious(new ReleaseRecordFixture().WithId(10).WithTag("v1").WithCommit(new string('1', 40)))
        .WithSelectionPolicy("publication-order-v1").WithCatalogDigest(new string('d', 64)).WithSelectingRunId("run-1");
}
