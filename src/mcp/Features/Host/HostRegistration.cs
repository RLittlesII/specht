using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Specht.Mcp.Features.Host;

/// <summary>Registers the host slice's application settings.</summary>
/// <implements epic="0132" feature="F3" claim="B-013" />
public static class HostRegistration
{
    /// <summary>Registers the root the session answers for, which the command states and every part reads.</summary>
    /// <param name="services">The host's registrations.</param>
    /// <returns><paramref name="services"/>, for the next part.</returns>
    public static IServiceCollection AddHost(this IServiceCollection services) => services.AddSingleton<SessionRoot>();

    /// <summary>Names the application and states its own package's version.</summary>
    /// <param name="configuration">The configurator.</param>
    /// <returns>The same configurator.</returns>
    /// <implements epic="0132" feature="F3" claim="B-013" />
    public static IConfigurator AddHost(this IConfigurator configuration)
    {
        configuration.SetApplicationName(HostCommand.Name);
        configuration.SetApplicationVersion(ThisAssembly.AssemblyInformationalVersion);

        return configuration;
    }
}
