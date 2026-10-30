using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Tool.Features;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Upgrade;

/// <summary>Builds the shipped files over the tool assembly's shipping copy until a test supplies its own.</summary>
internal sealed class ShippedFilesFixture : AutoFixtureBase<ShippedFilesFixture>
{
    public ShippedFilesFixture WithShippingCopy(IReadOnlyDictionary<string, byte[]> shippingCopy) => With(ref _shippingCopy, shippingCopy);

    public static implicit operator ShippedFiles(ShippedFilesFixture fixture) => new(fixture._shippingCopy);

    private IReadOnlyDictionary<string, byte[]> _shippingCopy = InitWriter.ShippingCopy(typeof(ShippedFiles).Assembly);
}
