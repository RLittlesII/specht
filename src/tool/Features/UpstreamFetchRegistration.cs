using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Specht.Manifest;

namespace Specht.Tool.Features;

/// <summary>Registers the credential-free HTTP client and the upstream fetcher once for every slice that fetches.</summary>
/// <implements epic="0001" feature="F7" claim="B-083" />
public static class UpstreamFetchRegistration
{
    /// <summary>
    /// Adds a client that sends no credential, cookie or redirect, and the fetcher over it, unless either is registered already
    /// (<c>0001-F7</c> C-15; <c>0001-F4</c> B-019).
    /// </summary>
    /// <param name="services">The host's services.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <implements epic="0001" feature="F7" claim="B-083" />
    /// <implements epic="0001" feature="F4" claim="B-019" />
    public static IServiceCollection AddUpstreamSchemaFetch(this IServiceCollection services)
    {
        services.TryAddSingleton(static _ => new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            UseDefaultCredentials = false,
            Credentials = null,
            DefaultProxyCredentials = null,
        }));
        services.TryAddSingleton<UpstreamSchemaFetcher>();
        return services;
    }
}
