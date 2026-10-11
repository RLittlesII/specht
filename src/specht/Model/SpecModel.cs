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
        Features = features;
        Items = items;
        Epics = epics;
        Schemas = schemas;
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
}
