using Spectre.Console.Cli;

namespace Specht.Tool.Features.Manifest;

/// <summary>Registers manifest inspection and its help description.</summary>
/// <implements epic="0001" feature="F9" claim="B-001" />
public static class ManifestRegistration
{
    /// <summary>Adds the manifest command.</summary>
    /// <implements epic="0001" feature="F9" claim="B-001" />
    public static IConfigurator AddManifest(this IConfigurator configuration)
    {
        configuration.AddCommand<ManifestCommand>("manifest").WithDescription("Print the effective manifest or shipped defaults.");
        return configuration;
    }
}
