using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Manifest;
using Specht.Tests.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>Builds an init writer with the shipping copy and an in-memory tree shared with its manifest store.</summary>
internal sealed class InitWriterFixture : AutoFixtureBase<InitWriterFixture>
{
    public InitWriterFixture WithShippingCopy(IReadOnlyDictionary<string, byte[]> shippingCopy) => With(ref _shippingCopy, shippingCopy);

    public InitWriterFixture WithFileSystem(IFileSystem fileSystem) => With(ref _fileSystem, fileSystem);

    public InitWriterFixture WithUpstreamSchemaFetcher(UpstreamSchemaFetcher upstreamSchemaFetcher) => With(ref _upstreamSchemaFetcher, upstreamSchemaFetcher);

    public static implicit operator InitWriter(InitWriterFixture fixture) => new(
        fixture._shippingCopy,
        fixture._fileSystem,
        EngineServices.Parser(),
        new SpecManifestStoreFixture().WithFileSystem(fixture._fileSystem),
        fixture._upstreamSchemaFetcher);

    private IReadOnlyDictionary<string, byte[]> _shippingCopy = InitWriter.ShippingCopy(typeof(InitWriter).Assembly);

    private IFileSystem _fileSystem = new MockFileSystem();

    private UpstreamSchemaFetcher? _upstreamSchemaFetcher;
}
