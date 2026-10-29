using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Tool;
using Specht.Tool.Features;
using Specht.Tool.Features.Upgrade;
using Spectre.Console;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Composes the engine graph and the upgrade slice's production registrations over an in-memory file system and an HTTP
/// client registered before them: the caller's synthetic source, or by default one whose handler fails any request, so no
/// upgrade test reaches the network (ADR-0013; <c>0001-F7</c> § 7 "Upgrade fetches the recorded source").
/// </summary>
internal static class UpgradeServices
{
    public static CommandAppTester App(IFileSystem fileSystem, TestConsole console, TextWriter? errors = null, HttpClient? source = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        services.AddSingleton<IAnsiConsole>(console);
        services.AddSingleton(source ?? new HttpClient(new NoNetwork()));
        if (errors is not null)
        {
            services.AddSingleton<TextWriter>(errors);
        }

        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()), console: console);
        app.Configure(static configuration => configuration.AddUpgrade());
        return app;
    }

    public static UpgradeWriter Writer(IFileSystem fileSystem, HttpClient? source = null) => Resolve<UpgradeWriter>(fileSystem, source);

    public static ShippedFiles Shipped() => Resolve<ShippedFiles>(new System.IO.Abstractions.TestingHelpers.MockFileSystem(), null);

    private static T Resolve<T>(IFileSystem fileSystem, HttpClient? source)
        where T : class
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        services.AddSingleton(source ?? new HttpClient(new NoNetwork()));
        return new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()).Build().Resolve(typeof(T)) as T
            ?? throw new InvalidOperationException($"AddUpgrade() registers no {typeof(T).Name}.");
    }

    private sealed class NoNetwork : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new InvalidOperationException($"An upgrade test asked for {request.RequestUri} without a synthetic source.");
    }
}
