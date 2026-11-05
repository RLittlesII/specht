using Microsoft.Extensions.DependencyInjection;
using Specht.Mcp.Features.Host;
using Specht.Mcp.Features.PinnedTool;
using Specht.Mcp.Features.Reading;
using Spectre.Console.Cli;

namespace Specht.Mcp;

/// <summary>
/// Composes the host (ADR-0007, <c>0132-F3</c> OQ-10): each slice registers itself and this factory lists them, so a test builds the
/// same host by calling it with what only the caller knows, the way programs are run.
/// </summary>
public static class HostFactory
{
    /// <summary>Builds the application the program runs.</summary>
    /// <param name="process">Starts the programs the host runs; the system's in the program, a fake in a test.</param>
    /// <returns>The application, not yet run.</returns>
    public static CommandApp<HostCommand> Create(IProcessRun process)
    {
        var services = new ServiceCollection()
            .AddSingleton(process)
            .AddHost()
            .AddPinnedTool()
            .AddReading();

        var app = new CommandApp<HostCommand>(new TypeRegistrar(services));
        app.Configure(static config => config.AddHost());

        return app;
    }
}
