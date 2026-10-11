using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Specht.Discovery;
using Specht.Model;
using Specht.Rules;

namespace Specht.Composition;

/// <summary>The engine's one registration (ADR-0001 stage D; ADR-0003).</summary>
public static class SpechtEngineServiceCollectionExtensions
{
    /// <summary>
    /// Registers every engine type as a singleton, each rule by name in ordinal order of its <see cref="ISpecRule.Id"/>,
    /// the order the report's violations are collected in (<c>0001-F1</c> B-004, C-9). A rule that is not registered
    /// here does not run, and a file system registered earlier is kept.
    /// </summary>
    /// <param name="services">The collection the engine joins.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddSpechtEngine(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<FrontmatterReader>();
        services.AddSingleton<FeatureFileReader>();
        services.AddSingleton<SpecDiscovery>();
        services.AddSingleton<SpecSchemasLoader>();
        services.AddSingleton<SpecModelLoader>();
        services.AddSingleton<SpechtRunner>();
        services.AddSingleton<ISpecRule, FrontmatterSchemaRule>();
        services.AddSingleton<ISpecRule, SectionStructureRule>();
        services.AddSingleton<ISpecRule, IdentityRule>();
        services.AddSingleton<ISpecRule, FeatureFileRule>();
        services.AddSingleton<ISpecRule, ClaimRule>();
        services.AddSingleton<ISpecRule, ChildItemRule>();
        services.AddSingleton<ISpecRule, DependencyRule>();
        services.AddSingleton<ISpecRule, ApprovalRule>();

        return services;
    }
}
