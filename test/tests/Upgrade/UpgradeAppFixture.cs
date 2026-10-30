using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Composition;
using Specht.Tool;
using Specht.Tool.Features.Upgrade;
using Spectre.Console;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Builds a command tester for <c>specht upgrade</c> over the engine graph and the upgrade slice's production registrations, an
/// in-memory file system, a capturing console, the test's error writer where it supplies one, and an HTTP client registered
/// before them: the test's synthetic source, or by default one whose handler fails any request (ADR-0013).
/// </summary>
internal sealed class UpgradeAppFixture : AutoFixtureBase<UpgradeAppFixture>
{
    public UpgradeAppFixture WithFileSystem(IFileSystem fileSystem) => With(ref _fileSystem, fileSystem);

    public UpgradeAppFixture WithErrors(TextWriter errors) => With(ref _errors, errors);

    public UpgradeAppFixture WithSource(HttpClient source) => With(ref _source, source);

    public static implicit operator CommandAppTester(UpgradeAppFixture fixture)
    {
        var console = new TestConsole();
        var services = new ServiceCollection();
        services.AddSingleton(fixture._fileSystem);
        services.AddSingleton<IAnsiConsole>(console);
        services.AddSingleton(fixture._source);
        if (fixture._errors is not null)
        {
            services.AddSingleton<TextWriter>(fixture._errors);
        }

        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()), console: console);
        app.Configure(static configuration => configuration.AddUpgrade());
        return app;
    }

    private IFileSystem _fileSystem = new MockFileSystem();

    private TextWriter? _errors;

    private HttpClient _source = new(new NoNetworkHandler());
}
