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

    /// <summary>Every discovered Feature specification, in both layouts.</summary>
    public IReadOnlyList<FeatureSpec> Features { get; }

    /// <summary>Every task, test, bug and spike file beside a specification.</summary>
    public IReadOnlyList<ChildItem> Items { get; }

    /// <summary>Every epic file.</summary>
    public IReadOnlyList<EpicFile> Epics { get; }

    /// <summary>The schemas and the section contract every rule is evaluated against.</summary>
    public SpecSchemas Schemas { get; }

    /// <summary>How many specifications are still in the legacy layout.</summary>
    public int LegacyCount => Features.Count(static feature => feature.Location.Layout == SpecLayout.Legacy);

    /// <summary>How many specifications have been migrated.</summary>
    public int CoLocatedCount => Features.Count(static feature => feature.Location.Layout == SpecLayout.CoLocated);

    /// <summary>Loads the model rooted at <paramref name="root"/>, the schemas and manifest before the tree.</summary>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestException">The manifest is rejected; nothing in the tree is read.</exception>
    public static SpecModel Load(string root)
    {
        var schemas = SpecSchemas.Load(new FileSystem(), root);
        var locations = SpecDiscovery.FindSpecifications(root);
        var features = new List<FeatureSpec>();

        foreach (var location in locations)
        {
            features.Add(new FeatureSpec(
                location,
                SpecDocument.Parse(File.ReadAllText(location.AbsolutePath), location.RelativePath),
                Directory.EnumerateFiles(location.Directory, "*.feature").Order(StringComparer.Ordinal).ToList()));
        }

        var items = new List<ChildItem>();

        foreach (var path in SpecDiscovery.FindChildItems(locations))
        {
            items.Add(new ChildItem(
                SpecDiscovery.Relative(root, path),
                Path.GetFileName(path),
                FrontmatterReader.Read(path),
                Path.GetDirectoryName(path)!));
        }

        var epics = new List<EpicFile>();
        var epicsDirectory = Path.Combine(root, "epics");

        if (Directory.Exists(epicsDirectory))
        {
            var paths = Directory.EnumerateFiles(epicsDirectory, "epic.md", SearchOption.AllDirectories);

            foreach (var path in paths.Order(StringComparer.Ordinal))
            {
                epics.Add(new EpicFile(SpecDiscovery.Relative(root, path), FrontmatterReader.Read(path)));
            }
        }

        return new SpecModel(root, features, items, epics, schemas);
    }
}
