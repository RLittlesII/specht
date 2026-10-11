using System.IO.Abstractions;
using LanguageExt;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Versioning;

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

    /// <summary>Loads the model rooted at <paramref name="root"/> with the embedded version set.</summary>
    /// <returns>The model, or the manifest's failure; on a failure nothing in the tree is read.</returns>
    public static Either<SpechtFailure, SpecModel> Load(string root) => Load(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the model rooted at <paramref name="root"/>, the manifest and the pinned version's schemas from
    /// <paramref name="versions"/> before the tree.
    /// </summary>
    /// <returns>
    /// The model, or the manifest's failure, one for a pin <paramref name="versions"/> does not hold included; on a failure
    /// nothing in the tree is read.
    /// </returns>
    public static Either<SpechtFailure, SpecModel> Load(string root, SchemaVersions versions)
    {
        var fileSystem = new FileSystem();

        return SpecSchemas.Load(fileSystem, root, versions).Map(schemas => Load(fileSystem, root, schemas));
    }

    private static SpecModel Load(FileSystem fileSystem, string root, SpecSchemas schemas)
    {
        var frontmatter = new FrontmatterReader(fileSystem);
        var discovery = schemas.Structure.Discovery;
        var locations = SpecDiscovery.FindSpecifications(fileSystem, root, discovery);
        var features = new List<FeatureSpec>();

        foreach (var location in locations)
        {
            features.Add(new FeatureSpec(
                location,
                SpecDocument.Parse(File.ReadAllText(location.AbsolutePath), location.RelativePath),
                SpecDiscovery.FindCompanions(fileSystem, location, discovery)));
        }

        var items = new List<ChildItem>();

        foreach (var path in SpecDiscovery.FindChildItems(fileSystem, locations, discovery, schemas.Structure.Identifiers["task"]))
        {
            items.Add(new ChildItem(
                SpecDiscovery.Relative(root, path),
                Path.GetFileName(path),
                frontmatter.Read(path),
                Path.GetDirectoryName(path)!));
        }

        var epics = new List<EpicFile>();

        foreach (var path in SpecDiscovery.FindEpics(fileSystem, root, discovery))
        {
            epics.Add(new EpicFile(SpecDiscovery.Relative(root, path), frontmatter.Read(path)));
        }

        return new SpecModel(root, features, items, epics, schemas);
    }
}
