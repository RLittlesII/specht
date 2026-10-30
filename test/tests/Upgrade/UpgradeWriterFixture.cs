using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Composition;
using Specht.Tool;
using Specht.Tool.Features.Upgrade;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Resolves the upgrade writer from the engine graph and the upgrade slice's production registrations over an in-memory file
/// system and an HTTP client registered before them: the test's synthetic source, or by default one whose handler fails any
/// request, so no upgrade test reaches the network (ADR-0013; <c>0001-F7</c> § 7 "Upgrade fetches the recorded source").
/// </summary>
internal sealed class UpgradeWriterFixture : AutoFixtureBase<UpgradeWriterFixture>
{
    public UpgradeWriterFixture WithFileSystem(IFileSystem fileSystem) => With(ref _fileSystem, fileSystem);

    public UpgradeWriterFixture WithSource(HttpClient source) => With(ref _source, source);

    public static implicit operator UpgradeWriter(UpgradeWriterFixture fixture)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fixture._fileSystem);
        services.AddSingleton(fixture._source);
        return new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()).Build().Resolve(typeof(UpgradeWriter)) as UpgradeWriter
            ?? throw new InvalidOperationException("AddUpgrade() registers no UpgradeWriter.");
    }

    private IFileSystem _fileSystem = new MockFileSystem();

    private HttpClient _source = new(new NoNetworkHandler());
}
