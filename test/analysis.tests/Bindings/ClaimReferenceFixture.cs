using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[AutoFixture(typeof(ClaimReference))]
internal sealed partial class ClaimReferenceFixture
{
    public ClaimReferenceFixture() => WithEpic("0400").WithFeature("F1").WithClaim("B-001");
}
