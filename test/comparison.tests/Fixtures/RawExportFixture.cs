using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(RawExport))]
internal sealed partial class RawExportFixture
{
    public RawExportFixture() => WithSide("current").WithSourcePath(ComparisonDocumentFixture.CurrentExport);
}
