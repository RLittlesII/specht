using Specht.Discovery;
using Specht.Model;
using Specht.Tree;

namespace Specht.Identifiers;

/// <summary>
/// Where the collision guard reads the ids a type already holds in the tree (ADR-0033's <c>HeldIds</c>): the file names in
/// every <c>adr/</c> folder for <c>adr</c> and in every <c>lessons/</c> folder for <c>lesson</c> (<c>0001-F19</c> B-038,
/// B-039), and for a per-Feature type the Feature's own places: the file names in its <c>.spec/decisions/</c>, the first
/// cell of every table row in its <c>.spec/README.md</c> and the tags of its companions (B-040 to B-042). Each is listed
/// through the check's own discovery and its exclusions, and never written (C-1, C-11). The root <c>.spec/</c>, which the
/// default exclusions skip as no specification, is read: it is where the repository-wide records live.
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
