using System.Collections.ObjectModel;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;

namespace Specht.Model;

/// <summary>The whole specification tree, resolved once and evaluated by every rule.</summary>
public sealed class SpecModel
{
    /// <summary>A model over what a caller already holds, such as documents built in memory.</summary>
    public SpecModel(
        string root,
        IReadOnlyList<FeatureSpec> features,
        IReadOnlyList<ChildItem> items,
        IReadOnlyList<EpicFile> epics,
        SpecSchemas schemas)
    {
        Root = root;
        Features = Array.AsReadOnly(features.ToArray());
        Items = Array.AsReadOnly(items.ToArray());
        Epics = Array.AsReadOnly(epics.ToArray());
        Schemas = schemas;
        Regex? grammar = null;
        try
        {
            grammar = new Regex(schemas.Structure.Identifiers["claim"], RegexOptions.Compiled);
        }
        catch (ArgumentException exception)
        {
            _claimGrammarFailure = ExceptionDispatchInfo.Capture(exception);
        }

        Claims = new ReadOnlyDictionary<FeatureSpec, SpecClaims>(Features.ToDictionary(
            static feature => feature, feature => new SpecClaims(feature.Document, schemas.Structure, grammar)));
        MatrixSections = new ReadOnlyDictionary<FeatureSpec, SpecSection?>(Features.ToDictionary(
            static feature => feature, feature => feature.Document.Section(schemas.Structure.Roles["matrix"])));
        FeaturesWithIdentity = Array.AsReadOnly(Features.Where(static feature => feature.Epic is not null && feature.Id is not null).ToArray());
        DependencyGraph = new SpecDependencyGraph(FeaturesWithIdentity);
    }

    /// <summary>Absolute path to the repository root.</summary>
    public string Root { get; }

    /// <summary>Every discovered Feature specification, in every layout.</summary>
    public IReadOnlyList<FeatureSpec> Features { get; }

    /// <summary>Every task, test, bug and spike file beside a specification.</summary>
    public IReadOnlyList<ChildItem> Items { get; }

    /// <summary>Every epic file.</summary>
    public IReadOnlyList<EpicFile> Epics { get; }

    /// <summary>The schemas and the section contract every rule is evaluated against.</summary>
    public SpecSchemas Schemas { get; }

    /// <summary>The claims derived once per Feature.</summary>
    public IReadOnlyDictionary<FeatureSpec, SpecClaims> Claims { get; }

    /// <summary>The configured matrix section of each Feature.</summary>
    public IReadOnlyDictionary<FeatureSpec, SpecSection?> MatrixSections { get; }

    /// <summary>Features with both identity fields, in discovery order.</summary>
    public IReadOnlyList<FeatureSpec> FeaturesWithIdentity { get; }

    /// <summary>The resolved dependency graph.</summary>
    public SpecDependencyGraph DependencyGraph { get; }

    /// <summary>Retains the legacy grammar failure only when a claim-reading rule actually runs.</summary>
    internal void EnsureClaimGrammar() => _claimGrammarFailure?.Throw();

    private readonly ExceptionDispatchInfo? _claimGrammarFailure;
}
