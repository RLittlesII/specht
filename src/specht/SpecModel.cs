using System.IO.Abstractions;

namespace specht;

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
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestException">The manifest is rejected; nothing in the tree is read.</exception>
    public static SpecModel Load(string root) => Load(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the model rooted at <paramref name="root"/>, the manifest and the pinned version's schemas from
    /// <paramref name="versions"/> before the tree.
    /// </summary>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestException">
    /// The manifest is rejected, or pins a version <paramref name="versions"/> does not hold; nothing in the tree is read.
    /// </exception>
    public static SpecModel Load(string root, SchemaVersions versions)
    {
        var fileSystem = new FileSystem();
        var schemas = SpecSchemas.Load(fileSystem, root, versions);
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
