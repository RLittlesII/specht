using Specht.Comparison.Tests.Collection;
using Specht.Comparison.Tests.Evidence;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Reporting;

internal static class ReportFixtures
{
    public static ReleasePairFixture Pair() => new ReleasePairFixture()
        .WithCurrent(new ReleaseRecordFixture().WithId(20).WithTag("v2").WithPublishedAt(new DateTimeOffset(2026, 1, 4, 0, 0, 0, TimeSpan.Zero)))
        .WithPrevious(new ReleaseRecordFixture().WithId(10).WithTag("v1").WithCommit(new string('e', 40)))
        .WithCatalogDigest(new string('f', 64));

    public static IdentityMappingFixture Mapping() => new IdentityMappingFixture()
        .WithFullName("Synthetic.Check(1)").WithProductCommit(new string('a', 40)).WithSourceFile("Check.cs")
        .WithSourceDigest(new string('d', 64)).WithCase(new WorkloadCaseFixture());
}
