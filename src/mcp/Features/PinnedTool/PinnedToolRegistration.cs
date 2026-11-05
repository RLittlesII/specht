using Microsoft.Extensions.DependencyInjection;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>Where this slice joins the host (ADR-0007).</summary>
public static class PinnedToolRegistration
{
    /// <summary>Registers the tool lookup over the system's processes; <see cref="IProcessRun"/> is the caller's to supply or replace.</summary>
    /// <param name="services">The host's registrations.</param>
    /// <returns><paramref name="services"/>, for the next part.</returns>
    public static IServiceCollection AddPinnedTool(this IServiceCollection services) => services.AddSingleton<SpechtTool>();
}
