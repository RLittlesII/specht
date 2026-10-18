using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[AutoFixture(typeof(BindingFeature))]
internal sealed partial class BindingFeatureFixture
{
    public BindingFeatureFixture() => WithIdentity(new FeatureIdentity("0400", "F1")).WithState("approved")
        .WithClaims(new BindingClaim[] { new BindingClaimFixture() }).WithRelativePath("src/one/.spec/README.md");

    public BindingFeatureFixture WithClaims(IReadOnlyList<BindingClaim> claims) => WithList(claims);
}
