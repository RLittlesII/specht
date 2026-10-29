using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Manifest;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Init;

/// <summary>Registers the init slice's services and command.</summary>
/// <implements epic="0001" feature="F4" claim="B-019" />
public static class InitRegistration
{
    /// <summary>Adds init over the engine's manifest services and the caller's HTTP client.</summary>
    /// <implements epic="0001" feature="F4" claim="B-019" />
    public static IServiceCollection AddInit(this IServiceCollection services)
    {
        services.AddUpstreamSchemaFetch();
        services.AddSingleton(static provider => new InitWriter(
            InitWriter.ShippingCopy(typeof(InitWriter).Assembly),
            provider.GetRequiredService<IFileSystem>(),
            provider.GetRequiredService<SpecManifestParser>(),
            provider.GetRequiredService<SpecManifestStore>(),
            provider.GetRequiredService<UpstreamSchemaFetcher>()));
        return services;
    }

    /// <summary>Adds the init command and its help description.</summary>
    /// <implements epic="0001" feature="F4" claim="B-019" />
    public static IConfigurator AddInit(this IConfigurator configuration)
    {
        configuration.AddCommand<InitCommand>("init")
            .WithDescription("Write the newest manifest and selected schemas and templates under .specht/, never overwriting a file.");
        return configuration;
    }
}
