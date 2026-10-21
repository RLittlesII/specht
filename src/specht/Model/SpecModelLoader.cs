using System.IO.Abstractions;
using Specht.Discovery;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>Resolves the specification tree under a root into the model every rule evaluates (ADR-0001 stage D; ADR-0005 (a)).</summary>
/// <param name="fileSystem">The file system the specifications are read through.</param>
/// <param name="frontmatter">The frontmatter reader.</param>
/// <param name="discovery">The discovery stage.</param>
/// <param name="schemas">The manifest stage.</param>
/// <param name="tags">The pure companion-tag parser.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpecModelLoader(
    IFileSystem fileSystem,
    FrontmatterReader frontmatter,
    SpecDiscovery discovery,
    SpecSchemasLoader schemas,
    FeatureTagParser tags)
{
    /// <summary>Loads the resolved tree through its injected manifest and schema stage.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecModel Load(string root) => Load(root, null);

    /// <summary>Loads the resolved tree with the frontmatter source selected for this invocation.</summary>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecModel Load(string root, SchemaSourceSelection? selection)
    {
        var loaded = schemas.Load(root, selection);
        var inputs = loaded.Structure.Discovery;
        var locations = discovery.FindSpecifications(root, inputs);
        var features = new List<FeatureSpec>();

        foreach (var location in locations)
        {
            features.Add(new FeatureSpec(
                location,
                SpecDocument.Parse(fileSystem.File.ReadAllText(location.AbsolutePath), location.RelativePath),
                discovery.FindCompanions(location, inputs)
                    .Select(path => new FeatureFile(Relative(root, path), tags.Parse(fileSystem.File.ReadAllText(path), loaded.Structure)))
                    .ToList()));
        }

        var items = new List<ChildItem>();

        foreach (var path in discovery.FindChildItems(locations, inputs, loaded.Structure.Identifiers["task"]))
        {
            items.Add(new ChildItem(
                Relative(root, path),
                fileSystem.Path.GetFileName(path),
                frontmatter.Read(path),
                fileSystem.Path.GetDirectoryName(path)!));
        }

        var epics = new List<EpicFile>();

        foreach (var path in discovery.FindEpics(root, inputs))
        {
            epics.Add(new EpicFile(Relative(root, path), frontmatter.Read(path)));
        }

        return new SpecModel(root, features, items, epics, loaded);
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private static string Relative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
}
