using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Specht.Tool.Features.Init;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Upgrade;

/// <summary>Registers the upgrade slice's services and command.</summary>
/// <implements epic="0001" feature="F7" claim="B-005" />
public static class UpgradeRegistration
{
    /// <summary>Adds upgrade over the engine's manifest services, the embedded shipped files and the upstream fetch.</summary>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    public static IServiceCollection AddUpgrade(this IServiceCollection services)
    {
        services.TryAddSingleton(static _ => new ShippedFiles(InitWriter.ShippingCopy(typeof(ShippedFiles).Assembly)));
        services.AddUpstreamSchemaFetch();
        services.AddSingleton<UpgradeWriter>();
        return services;
    }

    /// <summary>Adds the upgrade command and its help description.</summary>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    public static IConfigurator AddUpgrade(this IConfigurator configuration)
    {
        configuration.AddCommand<UpgradeCommand>("upgrade")
            .WithDescription(
                "Bring the present frontmatter schemas, templates and manifest keys to the pinned schema version, never moving the pin; "
                + "under schemaSource disk with no spechtSchema recorded, the schemas are left as they are and listed as skipped.");
        return configuration;
    }
}
