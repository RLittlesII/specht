using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using Specht.Discovery;

namespace Specht.Model;

/// <summary>The whole specification tree, resolved once and evaluated by every rule; every path in it is relative to a root it does not hold.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtModel
{
    /// <summary>A model over what a caller already holds, as <see cref="SpechtModelBuilder"/> or a test builds it.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-027" />
    /// <implements epic="0001" feature="F5" claim="B-069" />
    /// <implements epic="0001" feature="F5" claim="B-079" />
    /// <implements epic="0001" feature="F5" claim="B-086" />
    public SpechtModel(
        IReadOnlyList<FeatureSpec> features,
        IReadOnlyList<ChildItem> items,
        IReadOnlyList<EpicFile> epics,
        SpechtSchemas schemas,
        IReadOnlyList<SpechtTestFile> testFiles)
    {
        Features = Array.AsReadOnly(features.ToArray());
        Items = Array.AsReadOnly(items.ToArray());
        Epics = Array.AsReadOnly(epics.ToArray());
        Schemas = schemas;
        TestFiles = Array.AsReadOnly(testFiles.ToArray());
        Regex? grammar = null;
        try
        {
            grammar = new Regex(schemas.Structure.ClaimGrammar(null).Pattern, RegexOptions.Compiled);
        }
        catch (ArgumentException exception)
        {
            _claimGrammarFailure = ExceptionDispatchInfo.Capture(exception);
        }

        Claims = new ReadOnlyDictionary<FeatureSpec, SpechtClaims>(Features.ToDictionary(
            static feature => feature,
            feature => new SpechtClaims(
                feature.Document,
                schemas.Structure,
                feature.ClaimGrammar is { Prefix: not null } own ? new Regex(own.Pattern, RegexOptions.CultureInvariant) : grammar)));
        MatrixSections = new ReadOnlyDictionary<FeatureSpec, SpechtSection?>(Features.ToDictionary(
            static feature => feature, feature => feature.Document.Section(schemas.Structure.Roles["matrix"])));
        FeaturesWithIdentity = Array.AsReadOnly(Features
            .Where(feature => feature.Id is not null && (feature.Epic is not null || schemas.Structure.Forms is not null))
            .ToArray());
        DependencyGraph = new SpechtDependencyGraph(FeaturesWithIdentity, schemas.Structure);
    }

    /// <summary>Every discovered Feature specification, in every layout.</summary>
    public IReadOnlyList<FeatureSpec> Features { get; }

    /// <summary>Every task, test, bug and spike file beside a specification.</summary>
    public IReadOnlyList<ChildItem> Items { get; }

    /// <summary>Every epic file.</summary>
    public IReadOnlyList<EpicFile> Epics { get; }

    /// <summary>The schemas and the section contract every rule is evaluated against.</summary>
    public SpechtSchemas Schemas { get; }

    /// <summary>The test files the manifest's <c>tests.files</c> declares, read once and shared by every rule.</summary>
    public IReadOnlyList<SpechtTestFile> TestFiles { get; }

    /// <summary>The claims derived once per Feature.</summary>
    public IReadOnlyDictionary<FeatureSpec, SpechtClaims> Claims { get; }

    /// <summary>The claims of each claim file set, in the manifest's order; empty unless <c>claim.source</c> is <c>gherkin</c>.</summary>
    /// <implements epic="0001" feature="F16" claim="B-001" />
    public IReadOnlyList<SpechtClaims> ClaimSets { get; } = [];

    /// <summary>The configured matrix section of each Feature.</summary>
    public IReadOnlyDictionary<FeatureSpec, SpechtSection?> MatrixSections { get; }

    /// <summary>
    /// Features with an identity, in discovery order: each with an <c>id</c> from schema 0.3.0, and below it each with
    /// both an <c>epic</c> and an <c>id</c> (C-11; B-027, B-049, B-069).
    /// </summary>
    public IReadOnlyList<FeatureSpec> FeaturesWithIdentity { get; }

    /// <summary>The resolved dependency graph.</summary>
    public SpechtDependencyGraph DependencyGraph { get; }

    /// <summary>Retains the legacy grammar failure only when a claim-reading rule actually runs.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    internal void EnsureClaimGrammar() => _claimGrammarFailure?.Throw();

    private readonly ExceptionDispatchInfo? _claimGrammarFailure;
}
