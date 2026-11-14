using System.Text;
using System.Text.Json;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Tree;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>
/// The one type that sequences the stores for a check: it checks the root, reads and parses the manifest, selects and
/// compiles the frontmatter schemas, lists the tree, loads exactly what was listed and hands its text to the builder
/// (ADR-0010 § "I/O is confined").
/// </summary>
/// <param name="store">The manifest and local schema store.</param>
/// <param name="parser">The pure manifest parser and pin resolver.</param>
/// <param name="copy">The verifier of local upstream bytes.</param>
/// <param name="compiler">The schema compiler.</param>
/// <param name="discovery">The listing of the tree.</param>
/// <param name="tree">The store the listed files are read through.</param>
/// <param name="builder">The model builder.</param>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtModelReader(
    SpechtManifestStore store,
    SpechtManifestParser parser,
    UpstreamSchemaCopy copy,
    SpechtSchemaCompiler compiler,
    ISpecDiscovery discovery,
    SpechtTreeStore tree,
    SpechtModelBuilder builder)
{
    /// <summary>
    /// Reads the model of the tree under <paramref name="root"/> with the frontmatter source selected for this invocation;
    /// a tree read without an epic tier lists and reads no epic file (B-034).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="selection">This invocation's schema source, ahead of the manifest's, or null for the manifest's.</param>
    /// <returns>The model, every path in it relative to <paramref name="root"/>.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">The root holds no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest cannot be parsed.</exception>
    /// <exception cref="SpechtManifestException">The manifest or a selected schema is rejected.</exception>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F5" claim="B-034" />
    /// <implements epic="0001" feature="F16" claim="B-001" />
    /// <implements epic="0001" feature="F16" claim="B-004" />
    public SpechtModel Read(string root, SchemaSourceSelection? selection)
    {
        var schemas = ReadSchemas(root, selection);
        var structure = schemas.Structure;
        var inputs = structure.Discovery;
        var specifications = discovery.FindSpecifications(root, inputs);
        var companions = specifications.Select(location => discovery.FindCompanions(root, location, inputs)).ToArray();
        var items = discovery.FindChildItems(root, specifications, inputs, structure.Identifiers["task"]);
        IReadOnlyList<string> epics = structure.EpicLess ? [] : discovery.FindEpics(root, inputs);
        var tests = structure.Tests is { } declared ? discovery.FindTestFiles(root, declared.Files, inputs.Exclusions) : [];
        IReadOnlyList<IReadOnlyList<string>> claimFiles = structure.ClaimInput is { Source: SpechtClaimSource.Gherkin } input
            ? input.Files.Select(set => discovery.FindTestFiles(root, [set.Glob], inputs.Exclusions)
                .Where(static path => path.EndsWith(".feature", StringComparison.Ordinal))
                .ToArray()).ToArray()
            : [];
        var listing = new SpechtListing(specifications, companions, items, epics, tests) { ClaimFiles = claimFiles };
        var paths = specifications.Select(static location => location.RelativePath)
            .Concat(companions.SelectMany(static paths => paths))
            .Concat(items)
            .Concat(epics)
            .Concat(tests)
            .Concat(claimFiles.SelectMany(static paths => paths))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var texts = tree.Load(root, paths).Files.ToDictionary(static file => file.RelativePath, static file => file.Text(), StringComparer.Ordinal);

        return builder.Build(schemas, listing, texts);
    }

    /// <summary>
    /// Reads the manifest and the frontmatter schemas this invocation's selection, or else the manifest's, names: the
    /// embedded set, the local files, or a recorded upstream source's verified local copy (<c>0001-F7</c> B-024, B-077).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="selection">This invocation's schema source, or null for the manifest's.</param>
    /// <returns>The compiled schemas.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    /// <implements epic="0001" feature="F7" claim="B-053" />
    /// <implements epic="0001" feature="F7" claim="B-054" />
    public SpechtSchemas ReadSchemas(string root, SchemaSourceSelection? selection) => ReadSchemas(root, selection, parser.VersionInUse);

    /// <summary>
    /// Reads the manifest and the frontmatter schemas as <see cref="ReadSchemas(string, SchemaSourceSelection?)"/> does with
    /// the manifest's source, from the pinned version whatever its pin policy (<c>0001-F7</c> OQ-73, decision 0021).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The compiled schemas of the pinned version.</returns>
    /// <implements epic="0001" feature="F8" claim="B-006" />
    public SpechtSchemas ReadPinnedSchemas(string root) => ReadSchemas(root, null, parser.PinnedVersion);

    /// <summary>Checks the root, then reads and parses its manifest, keeping the missing-root, missing-file and shape failures.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The parsed manifest.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtStructure ReadManifest(string root) => ReadManifest(root, parser.Parse);

    /// <summary>Reads and validates the manifest after <paramref name="edit"/> changes its text, with the same failure handling.</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="edit">Changes the manifest's text before it is parsed.</param>
    /// <returns>The parsed manifest.</returns>
    /// <implements epic="0001" feature="F7" claim="B-020" />
    public SpechtStructure ReadManifest(string root, Func<string, string> edit) => ReadManifest(root, text => parser.Parse(edit(text)));

    /// <summary>Reads the check's validated manifest with its owned effective JSON, using the same store and failure handling.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The parsed structure and projected document.</returns>
    /// <implements epic="0001" feature="F9" claim="B-007" />
    public SpechtManifestDocument ReadManifestDocument(string root) => ReadManifest(root, parser.ParseDocument);

    /// <implements epic="0001" feature="F9" claim="B-007" />
    private T ReadManifest<T>(string root, Func<string, T> parse)
    {
        if (!store.RootExists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        if (!store.ManifestExists(root))
        {
            throw new SpechtManifestNotFoundException(store.RetiredManifestExists(root));
        }

        try
        {
            return parse(store.ReadManifest(root));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private SpechtSchemas ReadSchemas(string root, SchemaSourceSelection? selection, Func<SpechtStructure, SchemaVersion> select)
    {
        var structure = ReadManifest(root);
        var selected = select(structure);
        var source = structure.SpechtSchema;
        var effective = selection ?? structure.SchemaSource;
        if (source is null)
        {
            return effective == SchemaSourceSelection.Disk
                ? ReadDisk(root, structure, selected)
                : compiler.Compile(selected, structure, SpechtSchemaSource.Embedded);
        }

        if (effective == SchemaSourceSelection.Embedded)
        {
            throw new SpechtManifestException(
                $"{SpechtManifestStore.RelativePath}: schemaSource 'embedded' contradicts the recorded spechtSchema source.");
        }

        var files = source.Files.ToDictionary(
            static file => file.Kind, file => store.ReadFrontmatterSchema(root, file.FileName), StringComparer.Ordinal);
        var texts = copy.Verify(source, files);
        var version = selected with
        {
            FeatureSchema = texts["feature"],
            ItemSchema = texts["task"],
            EpicSchema = texts.GetValueOrDefault("epic", selected.EpicSchema),
        };
        return compiler.Compile(version, structure, SpechtSchemaSource.Upstream);
    }

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private SpechtSchemas ReadDisk(string root, SpechtStructure structure, SchemaVersion version) =>
        compiler.CompileDisk(version, structure, name => ReadDisk(root, name));

    /// <implements epic="0001" feature="F1" claim="B-004" />
    private string ReadDisk(string root, string name)
    {
        var path = SpechtManifestStore.FrontmatterSchemaFolder + "/" + name;
        try
        {
            var bytes = store.ReadFrontmatterSchema(root, name)
                ?? throw new SpechtManifestException($"{path}: the selected disk schema is absent.");
            return Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpechtManifestException($"{path}: the selected disk schema could not be read.");
        }
    }
}
