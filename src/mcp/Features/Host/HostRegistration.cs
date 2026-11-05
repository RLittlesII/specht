using Spectre.Console.Cli;

namespace Specht.Mcp.Features.Host;

/// <summary>Registers the host slice's application settings.</summary>
/// <implements epic="0132" feature="F3" claim="B-013" />
public static class HostRegistration
{
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
