using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Specht.Identifiers;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Mint;

/// <summary>Registers the mint slice's clock and command.</summary>
/// <implements epic="0001" feature="F19" claim="B-003" />
public static class MintRegistration
{
    /// <summary>Adds the machine's clock the Snowflake strategy reads, unless a clock is registered already.</summary>
    /// <param name="services">The host's services.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <implements epic="0001" feature="F19" claim="B-007" />
    public static IServiceCollection AddMint(this IServiceCollection services)
    {
        services.TryAddSingleton<IClock, SystemClock>();
        return services;
    }

    /// <summary>Adds the mint command and its help description.</summary>
    /// <implements epic="0001" feature="F19" claim="B-003" />
    public static IConfigurator AddMint(this IConfigurator configuration)
    {
        configuration.AddCommand<MintCommand>("mint").WithDescription("Print a new id for a type the manifest opts in.");
        return configuration;
    }
}
