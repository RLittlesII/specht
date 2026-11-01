using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Reporting;

[AutoFixture(typeof(RawExport))]
internal sealed partial class RawExportFixture
{
    public RawExportFixture() => WithSide("current").WithSourcePath(ComparisonDocumentFixture.CurrentExport);
}
