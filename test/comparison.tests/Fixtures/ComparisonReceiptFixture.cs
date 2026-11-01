using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Reporting;

namespace Specht.Comparison.Tests.Workflow;

[AutoFixture(typeof(ComparisonReceipt))]
internal sealed partial class ComparisonReceiptFixture
{
    public ComparisonReceiptFixture() => WithFormatVersion(1).WithRepository("example/tool").WithRunId("1234").WithAttempt(2)
        .WithCommit(new string('a', 40)).WithReleaseId(20).WithPairDigest(ComparisonJson.Digest((ReleasePair)ReportFixtures.Pair()));
}
