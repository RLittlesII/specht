using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[AutoFixture(typeof(BindingClaim))]
internal sealed partial class BindingClaimFixture
{
    public BindingClaimFixture() => WithId("B-001").WithState("Active").WithLine(12).WithReplacementId(null).WithCoverageExemption(null);
}
