using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[AutoFixture(typeof(AdrReference))]
internal sealed partial class AdrReferenceFixture
{
    public AdrReferenceFixture() => WithEpic(null).WithFeature(null).WithAdr("ADR-0001");
}
