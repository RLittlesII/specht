using System.IO.Abstractions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>Resolves the specification tree under a root into the model every rule evaluates (ADR-0001 stage D; ADR-0005 (a)).</summary>
/// <param name="fileSystem">The file system the specifications are read through.</param>
/// <param name="frontmatter">The frontmatter reader.</param>
/// <param name="discovery">The discovery stage.</param>
/// <param name="schemas">The manifest stage.</param>
public sealed class SpecModelLoader(
    IFileSystem fileSystem,
    FrontmatterReader frontmatter,
    SpecDiscovery discovery,
    SpecSchemasLoader schemas)
{
    /// <summary>Loads the model rooted at <paramref name="root"/> with the embedded version set.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The resolved tree.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestException">The manifest is rejected; nothing in the tree is read.</exception>
    public SpecModel Load(string root) => Load(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the model rooted at <paramref name="root"/>, the manifest and the pinned version's schemas from
    /// <paramref name="versions"/> before the tree.
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="versions">The version set the pin selects from.</param>
    /// <returns>The resolved tree.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse; nothing in the tree is read.</exception>
    /// <exception cref="SpechtManifestException">
    /// The manifest is rejected, or pins a version <paramref name="versions"/> does not hold; nothing in the tree is read.
    /// </exception>
    public SpecModel Load(string root, SchemaVersions versions)
    {
        var loaded = schemas.Load(root, versions);
        var inputs = loaded.Structure.Discovery;
        var locations = discovery.FindSpecifications(root, inputs);
        var features = new List<FeatureSpec>();

        foreach (var location in locations)
        {
            features.Add(new FeatureSpec(
                location,
                SpecDocument.Parse(fileSystem.File.ReadAllText(location.AbsolutePath), location.RelativePath),
                discovery.FindCompanions(location, inputs)));
        }

        var items = new List<ChildItem>();

        foreach (var path in discovery.FindChildItems(locations, inputs, loaded.Structure.Identifiers["task"]))
        {
            items.Add(new ChildItem(
                SpecPath.Relative(root, path),
                fileSystem.Path.GetFileName(path),
                frontmatter.Read(path),
                fileSystem.Path.GetDirectoryName(path)!));
        }

        var epics = new List<EpicFile>();

        foreach (var path in discovery.FindEpics(root, inputs))
        {
            epics.Add(new EpicFile(SpecPath.Relative(root, path), frontmatter.Read(path)));
        }

        return new SpecModel(root, features, items, epics, loaded);
    }
}
