using Spectre.Console.Cli;

namespace Specht.Tool.Features.Mint;

/// <summary>Registers the mint command.</summary>
/// <implements epic="0001" feature="F19" claim="B-003" />
public static class MintRegistration
{
    /// <summary>Adds the mint command and its help description.</summary>
    /// <implements epic="0001" feature="F19" claim="B-003" />
    public static IConfigurator AddMint(this IConfigurator configuration)
    {
        configuration.AddCommand<MintCommand>("mint").WithDescription("Print a new id for a type the manifest opts in.");
        return configuration;
    }
}
