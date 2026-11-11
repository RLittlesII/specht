using Specht.Discovery;
using Specht.Model;
using Specht.Tree;

namespace Specht.Identifiers;

/// <summary>
/// Where the collision guard reads the ids a type already holds in the tree (ADR-0033's <c>HeldIds</c>): the file names in
/// every <c>adr/</c> folder for <c>adr</c> and in every <c>lessons/</c> folder for <c>lesson</c> (<c>0001-F19</c> B-038,
/// B-039), listed through the check's own discovery and its exclusions, and never written (C-1, C-11). The root
/// <c>.spec/</c>, which the default exclusions skip as no specification, is read: it is where the repository-wide records live.
/// </summary>
/// <param name="discovery">The listing of the tree.</param>
/// <param name="tree">The store a Feature's files are read through.</param>
/// <param name="documents">The parser of a Feature's specification tables.</param>
/// <param name="tags">The parser of a Feature's companion tags.</param>
/// <implements epic="0001" feature="F19" claim="B-038" />
/// <implements epic="0001" feature="F19" claim="B-039" />
public sealed class HeldIds(ISpecDiscovery discovery, SpechtTreeStore tree, SpechtDocumentParser documents, FeatureTagParser tags)
{
    /// <summary>The specification of the Feature a folder, relative to the root, holds, as the check lists it (<c>0001-F19</c> A-7).</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs.</param>
    /// <param name="folder">The folder, relative to the root.</param>
    /// <returns>The Feature's specification, or <see langword="null"/> when the folder holds none.</returns>
    /// <implements epic="0001" feature="F19" claim="B-059" />
    /// <implements epic="0001" feature="F19" claim="B-060" />
    public SpechtLocation? FeatureAt(string root, SpechtDiscoveryInputs inputs, string folder) => null;

    /// <summary>
    /// Lists the name of each file in the folders a type's ids are held in, outside the manifest's exclusions less the root
    /// <c>.spec/</c> skip.
    /// </summary>
    /// <param name="type">The id type.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="inputs">The manifest's discovery inputs, whose exclusions the listing honours.</param>
    /// <param name="feature">The specification of the Feature a per-Feature type is minted for, or nothing.</param>
    /// <returns>The held names, none for a type whose places are not read.</returns>
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    /// <implements epic="0001" feature="F19" claim="B-041" />
    /// <implements epic="0001" feature="F19" claim="B-042" />
    public IReadOnlyList<string> Of(string type, string root, SpechtDiscoveryInputs inputs, SpechtLocation? feature) => (type, feature) switch
    {
        ("claim" or "constraint" or "openQuestion", { } spec) => documents.FirstCells(Texts(root, [spec.RelativePath]).Single()),
        ("scenario", { } spec) => [.. Texts(root, discovery.FindCompanions(root, spec, inputs)).SelectMany(tags.TagNames)],
        _ => Names(type, root, inputs.Exclusions),
    };

    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    private string[] Names(string type, string root, IReadOnlyList<string> exclusions)
    {
        IReadOnlyList<string> places = type switch
        {
            "adr" => ["**/adr/*"],
            "lesson" => ["**/lessons/*"],
            _ => [],
        };

        return discovery.FindTestFiles(root, places, exclusions.Where(static entry => entry != RootSpec).ToArray())
            .Select(static path => path[(path.LastIndexOf('/') + 1)..])
            .ToArray();
    }

    /// <implements epic="0001" feature="F19" claim="B-041" />
    /// <implements epic="0001" feature="F19" claim="B-042" />
    private IEnumerable<string> Texts(string root, IReadOnlyList<string> paths) =>
        tree.Load(root, paths).Files.Select(static file => file.Text());

    /// <summary>The exclusion that skips the root <c>.spec/</c> (<c>0001-F6</c> decision 0003), which the guard does not apply.</summary>
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    private const string RootSpec = "/.spec";
}
