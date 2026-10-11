using System.IO.Abstractions;
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
    /// <returns>The model, or the manifest's <see cref="InputFailure"/> with nothing in the tree read.</returns>
    public static Outcome<SpecModel> Load(string root) => Load(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the model rooted at <paramref name="root"/>, the manifest and the pinned version's schemas from
    /// <paramref name="versions"/> before the tree.
    /// </summary>
    /// <returns>
    /// The model, or the manifest's <see cref="InputFailure"/> with nothing in the tree read: a
    /// <see cref="ManifestRejected"/> also when the manifest pins a version <paramref name="versions"/> does not hold.
    /// </returns>
    public static Outcome<SpecModel> Load(string root, SchemaVersions versions)
    {
        var fileSystem = new FileSystem();

        return SpecSchemas.Load(fileSystem, root, versions) switch
        {
            SpecSchemas schemas => Resolved(fileSystem, root, schemas),
            InputFailure failure => failure,
        };
    }

    private static SpecModel Resolved(FileSystem fileSystem, string root, SpecSchemas schemas)
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
