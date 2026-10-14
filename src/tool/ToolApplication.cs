using System;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Manifest;
using Specht.Tool.Features.Check;
using Specht.Tool.Features.Init;
using Specht.Tool.Features.Schema;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool;

/// <summary>Composes the graph shared by the host and command tests (ADR-0007).</summary>
public static class ToolApplication
{
    /// <summary>Creates the registrar and configuration shared by both command-app wrappers.</summary>
    public static (ITypeRegistrar Registrar, Action<IConfigurator> Configure) Create(IFileSystem fileSystem, IAnsiConsole? console = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(fileSystem);
        if (console is not null)
        {
            services.AddSingleton(console);
        }
        services.AddSpechtEngine();
        services.AddSingleton<Func<string, SchemaSourceSelection?, SpechtReport>>(
            static provider => provider.GetRequiredService<SpechtRunner>().Run);
        services.AddInit();
        return (new TypeRegistrar(services), static configuration =>
        {
            configuration.SetApplicationVersion(ThisAssembly.AssemblyInformationalVersion);
            configuration.AddInit().AddSchema();
        }
        );
    }

    /// <summary>Creates the real command app over the caller's file system.</summary>
    public static CommandApp CreateApp(IFileSystem fileSystem)
    {
        var (registrar, configure) = Create(fileSystem);
        var app = new CommandApp(registrar);
        app.SetDefaultCommand<CheckCommand>();
        app.Configure(configure);
        return app;
    }
}
