using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>Builds a manifest store over an empty in-memory file system until a test supplies its tree.</summary>
internal sealed class SpechtManifestStoreFixture : AutoFixtureBase<SpechtManifestStoreFixture>
{
    public SpechtManifestStoreFixture WithFileSystem(IFileSystem fileSystem) => With(ref _fileSystem, fileSystem);

    public static implicit operator SpechtManifestStore(SpechtManifestStoreFixture fixture) => new(fixture._fileSystem);

    private IFileSystem _fileSystem = new MockFileSystem();
}
