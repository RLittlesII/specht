using System.Collections.Immutable;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Specht.Comparison.Tests.Evidence;

[AutoFixture(typeof(MeasurementInventory))]
internal sealed partial class MeasurementInventoryFixture
{
    public MeasurementInventoryFixture() => WithKnown(true).WithCases([new CaseEvidenceFixture()]).WithDiagnostics([]);
}
