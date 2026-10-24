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

/// <summary>Composes the engine graph and the upgrade slice's production registrations over an in-memory file system.</summary>
internal static class UpgradeServices
{
    public static CommandAppTester App(IFileSystem fileSystem, TestConsole console)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        services.AddSingleton<IAnsiConsole>(console);
        var app = new CommandAppTester(new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()), console: console);
        app.Configure(static configuration => configuration.AddUpgrade());
        return app;
    }

    public static UpgradeWriter Writer(IFileSystem fileSystem) => Resolve<UpgradeWriter>(fileSystem);

    public static ShippedFiles Shipped() => Resolve<ShippedFiles>(new System.IO.Abstractions.TestingHelpers.MockFileSystem());

    private static T Resolve<T>(IFileSystem fileSystem)
        where T : class
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        return new TypeRegistrar(services.AddSpechtEngine().AddUpgrade()).Build().Resolve(typeof(T)) as T
            ?? throw new InvalidOperationException($"AddUpgrade() registers no {typeof(T).Name}.");
    }
}
