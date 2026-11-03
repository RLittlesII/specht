using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Tests.Reporting;

namespace Specht.Comparison.Tests.Workflow;

[AutoFixture(typeof(ReportReference))]
internal sealed partial class ReportReferenceFixture
{
    public ReportReferenceFixture() => WithRelease(((ReleasePair) ReportFixtures.Pair()).Current)
        .WithBundle(new PublishedBundle(1000, "comparison-20-1234-2.zip", new string('d', 64)));
}
