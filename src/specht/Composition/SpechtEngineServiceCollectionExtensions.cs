using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Specht.Check;
using Specht.Discovery;
using Specht.Evaluation;
using Specht.Identifiers;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Resources;
using Specht.Rules;
using Specht.Rules.Matrix;
using Specht.Tree;
using Specht.Versioning;

namespace Specht.Composition;

/// <summary>The engine's one registration (ADR-0001 stage D; ADR-0003).</summary>
/// <implements epic="0001" feature="F2" claim="B-001" />
public static class SpechtEngineServiceCollectionExtensions
{
    /// <summary>
    /// Registers every engine type as a singleton, each rule by name in ordinal order of its <see cref="ISpechtRule.Id"/>,
    /// the order the report's violations are collected in (<c>0001-F1</c> B-004, C-9). A rule that is not registered
    /// here does not run, and a file system or random source registered earlier is kept.
    /// </summary>
    /// <param name="services">The collection the engine joins.</param>
    /// <returns><paramref name="services"/>.</returns>
    /// <implements epic="0001" feature="F2" claim="B-001" />
    public static IServiceCollection AddSpechtEngine(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<SpechtResources>();
        services.TryAddSingleton(static provider => new SchemaVersions(provider.GetRequiredService<SpechtResources>().Texts));
        services.AddSingleton<SpechtManifestParser>();
        services.AddSingleton<FrontmatterReader>();
        services.AddSingleton<SpechtDocumentParser>();
        services.AddSingleton<FeatureTagParser>();
        services.TryAddSingleton<IChildProcess, ChildProcess>();
        services.AddSingleton<ISpecDiscovery, SpechtDiscovery>();
        services.AddSingleton<MatrixResolver>();
        services.AddSingleton<SpechtManifestStore>();
        services.AddSingleton<ManifestPinEditor>();
        services.AddSingleton<ManifestRuleEditor>();
        services.AddSingleton<ManifestKeyEditor>();
        services.AddSingleton<ManifestIdMover>();
        services.AddSingleton<RuleLevelSetter>();
        services.AddSingleton<SchemaVersionManager>();
        services.AddSingleton<UpstreamSchemaCopy>();
        services.AddSingleton<SpechtSchemaCompiler>();
        services.AddSingleton<RuleExplanationReader>();
        services.AddSingleton<RuleEntriesReader>();
        services.AddSingleton<EffectiveManifestReader>();
        services.AddSingleton<SpechtModelBuilder>();
        services.AddSingleton<SpechtModelReader>();
        services.AddSingleton<SpechtViolationOrder>();
        services.AddSingleton<SpechtEvaluator>();
        services.AddSingleton<SpechtCheck>();
        services.AddSingleton<SpechtFrontmatterCheck>();
        services.AddSingleton<SpechtReportDocumentMapper>();
        services.AddSingleton<SpechtReportSerializer>();
        services.AddSingleton<SpechtReportStore>();
        services.AddSingleton<SpechtTreeStore>();
        services.AddSingleton<SpechtTreeGenerator>();
        services.AddSingleton<FrontmatterSchemaCheck>();
        services.TryAddSingleton<IRandomSource, RandomSource>();
        services.AddSingleton<HeldIds>();
        services.AddSingleton<Minter>();
        services.AddSingleton<ISpechtRule, FrontmatterPresentRule>();
        services.AddSingleton<ISpechtRule, FeatureFrontmatterRule>();
        services.AddSingleton<ISpechtRule, ItemFrontmatterRule>();
        services.AddSingleton<ISpechtRule, EpicFrontmatterRule>();
        services.AddSingleton<ISpechtRule, SectionContractRule>();
        services.AddSingleton<ISpechtRule, PathIdentityRule>();
        services.AddSingleton<ISpechtRule, DuplicateIdentityRule>();
        services.AddSingleton<ISpechtRule, TableHeaderRule>();
        services.AddSingleton<ISpechtRule, CompanionFileRule>();
        services.AddSingleton<ISpechtRule, ClaimTagRule>();
        services.AddSingleton<ISpechtRule, ScenarioTitleRule>();
        services.AddSingleton<ISpechtRule, MissingScenarioRule>();
        services.AddSingleton<ISpechtRule, ClaimIdRule>();
        services.AddSingleton<ISpechtRule, ClaimRowRule>();
        services.AddSingleton<ISpechtRule, UnresolvedTestRule>();
        services.AddSingleton<ISpechtRule, StatusRule>();
        services.AddSingleton<ISpechtRule, WaiverRule>();
        services.AddSingleton<ISpechtRule, RetiredClaimRule>();
        services.AddSingleton<DeclaredItemCheck>();
        services.AddSingleton<ISpechtRule, DeclaredChildrenRule>();
        services.AddSingleton<ISpechtRule, DeclaredSpikesRule>();
        services.AddSingleton<ISpechtRule, ItemPlacementRule>();
        services.AddSingleton<ISpechtRule, ItemSequenceRule>();
        services.AddSingleton<ISpechtRule, DependencyTargetRule>();
        services.AddSingleton<ISpechtRule, DependencySymmetryRule>();
        services.AddSingleton<ISpechtRule, DependencyCycleRule>();
        services.AddSingleton<ISpechtRule, ApprovalCoverageRule>();
        services.AddSingleton<ISpechtRule, ApprovalSignOffRule>();

        services.Decorate<ISpechtRule, FaultIsolatedRule>();

        return services;
    }
}
