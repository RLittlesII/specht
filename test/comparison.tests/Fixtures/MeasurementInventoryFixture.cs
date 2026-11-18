using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;

namespace Specht.Comparison.Tests.Fixtures;

[AutoFixture(typeof(MeasurementInventory))]
internal sealed partial class MeasurementInventoryFixture
{
    public MeasurementInventoryFixture() => WithKnown(true).WithCases([new CaseEvidenceFixture()]).WithDiagnostics([]);
}
