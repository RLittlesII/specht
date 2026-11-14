using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Tree;

namespace Specht.Identifiers;

/// <summary>
/// Where the collision guard reads the ids a type already holds in the tree (ADR-0033's <c>HeldIds</c>): the file names in
/// every <c>adr/</c> folder for <c>adr</c> and in every <c>lessons/</c> folder for <c>lesson</c> (<c>0001-F19</c> B-038,
/// B-039), and for a per-Feature type the Feature's own places: the file names in its <c>.spec/decisions/</c>, the first
/// cell of every table row in its <c>.spec/README.md</c> and the tags of its companions (B-040 to B-042), and for a
/// <c>claim</c> under a <c>gherkin</c> claim source also the claims the check builds from the claim file sets of the
/// Feature's identity or whose <c>record</c> is the Feature's specification (B-078). Each is listed through the check's own
/// discovery and its exclusions, and never written (C-1, C-11). The root <c>.spec/</c>, which the default exclusions skip as
/// no specification, is read: it is where the repository-wide records live.
/// </summary>
/// <param name="discovery">The listing of the tree.</param>
/// <param name="tree">The store a Feature's files are read through.</param>
/// <param name="documents">The parser of a Feature's specification tables.</param>
/// <param name="tags">The parser of a Feature's companion tags.</param>
/// <implements epic="0001" feature="F19" claim="B-038" />
/// <implements epic="0001" feature="F19" claim="B-039" />
/// <implements epic="0001" feature="F19" claim="B-040" />
/// <implements epic="0001" feature="F19" claim="B-041" />
/// <implements epic="0001" feature="F19" claim="B-042" />
/// <implements epic="0001" feature="F19" claim="B-078" />
public sealed class HeldIds(ISpecDiscovery discovery, SpechtTreeStore tree, SpechtDocumentParser documents, FeatureTagParser tags)
{
    /// <summary>The specification of the Feature a folder, relative to the root, holds, as the check lists it (<c>0001-F19</c> A-7).</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <param name="folder">The folder, relative to the root.</param>
    /// <returns>The Feature's specification, or <see langword="null"/> when the folder holds none.</returns>
    /// <implements epic="0001" feature="F19" claim="B-059" />
    /// <implements epic="0001" feature="F19" claim="B-060" />
    public SpechtLocation? FeatureAt(string root, SpechtDiscoveryInputs inputs, string folder) =>
        discovery.FindSpecifications(root, inputs)
            .FirstOrDefault(location => string.Equals(location.RelativePath, folder.TrimEnd('/') + "/.spec/README.md", StringComparison.Ordinal));

    /// <summary>
    /// Lists the names a type's ids are held by: for <c>adr</c>, <c>lesson</c> and <c>decision</c> the name of each file in the
    /// folders they are held in, outside the manifest's exclusions less the root <c>.spec/</c> skip; for <c>claim</c>,
    /// <c>constraint</c> and <c>openQuestion</c> the first cells of the Feature's tables; and for <c>scenario</c> its companions'
    /// tag names.
    /// </summary>
    /// <param name="type">The id type.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs, whose exclusions the listing honours.</param>
    /// <param name="feature">The specification of the Feature a per-Feature type is minted for, or nothing.</param>
    /// <returns>The held names, none for a type whose places are not read.</returns>
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    /// <implements epic="0001" feature="F19" claim="B-040" />
    /// <implements epic="0001" feature="F19" claim="B-041" />
    /// <implements epic="0001" feature="F19" claim="B-042" />
    public IReadOnlyList<string> Of(string type, string root, SpechtDiscoveryInputs inputs, SpechtLocation? feature) => (type, feature) switch
    {
        ("adr", _) => Names(["**/adr/*"], root, inputs.Exclusions),
        ("lesson", _) => Names(["**/lessons/*"], root, inputs.Exclusions),
        ("decision", { } spec) => Names([spec.Directory + "/decisions/*"], root, inputs.Exclusions),
        ("claim" or "constraint" or "openQuestion", { } spec) => documents.FirstCells(Texts(root, [spec.RelativePath]).Single()),
        ("scenario", { } spec) => [.. Texts(root, discovery.FindCompanions(root, spec, inputs)).SelectMany(tags.TagNames)],
        _ => [],
    };

    /// <summary>
    /// The ids of the claims the check builds (<c>0001-F16</c> B-001) from the claim file sets whose Feature identity is the
    /// one <paramref name="feature"/>'s <c>.spec/README.md</c> frontmatter states: the set's <c>epic</c> and <c>feature</c>
    /// equal to the Feature's epic and id, or its <c>feature</c> alone where the manifest declares no epic grammar and so no
    /// set has an <c>epic</c> (<c>0001-F16</c> B-027), and also the sets whose <c>record</c>, resolved by
    /// <see cref="SpechtTreeStore.TryLoad"/> as the check resolves it, is that <c>.spec/README.md</c>, whatever identity they
    /// declare (<c>0001-F16</c> B-035, B-037). A <c>record</c> the check reads as none leaves the set matched by identity alone,
    /// and another Feature's sets hold nothing for this one (<c>0001-F19</c> § 10, readings of B-078).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="structure">The manifest, whose <c>claim</c> members, Feature keys and exclusions are read.</param>
    /// <param name="feature">The specification of the Feature a <c>claim</c> id is minted for.</param>
    /// <returns>The claim ids, none unless <c>claim.source</c> is <c>gherkin</c>.</returns>
    /// <implements epic="0001" feature="F19" claim="B-078" />
    public IReadOnlyList<string> NativeClaimsOf(string root, SpechtStructure structure, SpechtLocation feature)
    {
        if (structure.ClaimInput is not { Source: SpechtClaimSource.Gherkin } input)
        {
            return [];
        }

        var frontmatter = documents.Parse(Texts(root, [feature.RelativePath]).Single(), feature.RelativePath).Frontmatter;
        var id = frontmatter.Text(structure.FeatureKeys.Id);
        var epic = frontmatter.Text(structure.FeatureKeys.Epic);
        var sets = input.Files
            .Where(set => (string.Equals(set.Feature, id, StringComparison.Ordinal)
                    && (set.Epic is null || string.Equals(set.Epic, epic, StringComparison.Ordinal)))
                || (set.Record is { } record
                    && string.Equals(tree.TryLoad(root, record)?.RelativePath, feature.RelativePath, StringComparison.Ordinal)))
            .ToArray();
        var files = sets.Select(set => discovery.FindClaimFiles(root, set.Glob, structure.Discovery.Exclusions)).ToArray();
        var texts = tree.Load(root, files.SelectMany(static paths => paths).Distinct(StringComparer.Ordinal).ToArray()).Files
            .ToDictionary(static file => file.RelativePath, static file => file.Text(), StringComparer.Ordinal);

        return [.. SpechtModelBuilder.ClaimSets(structure, sets, files, texts).SelectMany(static claims => claims.Rows).Select(static claim => claim.Id)];
    }

    /// <summary>
    /// The claim prefix a Feature declares: the text its <c>.spec/README.md</c> frontmatter holds under <paramref name="key"/>,
    /// read as the check reads it (<c>0001-F19</c> A-9).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="feature">The Feature's specification.</param>
    /// <param name="key">The frontmatter key the manifest names for the claim prefix, or nothing.</param>
    /// <returns>The text under the key, or <see langword="null"/> for no key, no value or a value that is not text.</returns>
    /// <implements epic="0001" feature="F19" claim="B-063" />
    /// <implements epic="0001" feature="F19" claim="B-066" />
    /// <implements epic="0001" feature="F19" claim="B-067" />
    public string? ClaimPrefixOf(string root, SpechtLocation feature, string? key) =>
        key is null ? null : documents.Parse(Texts(root, [feature.RelativePath]).Single(), feature.RelativePath).Frontmatter.Text(key);

    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    /// <implements epic="0001" feature="F19" claim="B-040" />
    private string[] Names(IReadOnlyList<string> places, string root, IReadOnlyList<string> exclusions) =>
        discovery.FindTestFiles(root, places, exclusions.Where(static entry => entry != RootSpec).ToArray())
            .Select(static path => path[(path.LastIndexOf('/') + 1)..])
            .ToArray();

    /// <implements epic="0001" feature="F19" claim="B-041" />
    /// <implements epic="0001" feature="F19" claim="B-042" />
    private IEnumerable<string> Texts(string root, IReadOnlyList<string> paths) =>
        tree.Load(root, paths).Files.Select(static file => file.Text());

    /// <summary>The exclusion that skips the root <c>.spec/</c> (<c>0001-F6</c> decision 0003), which the guard does not apply.</summary>
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    private const string RootSpec = "/.spec";
}
