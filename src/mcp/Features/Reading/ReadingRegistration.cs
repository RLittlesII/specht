using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Specht.Mcp.Features.Reading;

/// <summary>Where this slice joins the host (ADR-0007).</summary>
public static class ReadingRegistration
{
    /// <summary>Registers the answers the host serves, each as a tool the host lists.</summary>
    /// <param name="services">The host's registrations.</param>
    /// <returns><paramref name="services"/>, for the next part.</returns>
    public static IServiceCollection AddReading(this IServiceCollection services) =>
        services
            .AddSingleton<CheckTool>()
            .AddSingleton(static provider => provider.GetRequiredService<CheckTool>().Declare())
            .AddSingleton<ExplainTool>()
            .AddSingleton(static provider => provider.GetRequiredService<ExplainTool>().Declare())
            .AddSingleton<RulesTool>()
            .AddSingleton(static provider => provider.GetRequiredService<RulesTool>().Declare())
            .AddSingleton<ManifestTool>()
            .AddSingleton(static provider => provider.GetRequiredService<ManifestTool>().Declare())
            .AddSingleton<MintTool>()
            .AddSingleton(static provider => provider.GetRequiredService<MintTool>().Declare());
}
