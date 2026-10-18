using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[AutoFixture(typeof(BindingAdr))]
internal sealed partial class BindingAdrFixture
{
    public BindingAdrFixture() => WithId("ADR-0001").WithOwner(null).WithState("accepted")
        .WithRelativePath(".spec/adr/0001-choice.md").WithLine(1).WithReplacementId(null);
}
