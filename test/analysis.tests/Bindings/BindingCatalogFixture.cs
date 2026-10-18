using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Tests.Bindings;

[AutoFixture(typeof(BindingCatalog))]
internal sealed partial class BindingCatalogFixture
{
    public BindingCatalogFixture() => WithFeatures(Array.Empty<BindingFeature>()).WithAdrs(Array.Empty<BindingAdr>());

    public BindingCatalogFixture WithFeatures(IReadOnlyList<BindingFeature> features) => WithList(features);

    public BindingCatalogFixture WithAdrs(IReadOnlyList<BindingAdr> adrs) => WithList(adrs);
}
