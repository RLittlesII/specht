using Specht.Discovery;

namespace Specht.Identifiers;

/// <summary>
/// Where the collision guard reads the ids a type already holds in the tree (ADR-0033's <c>HeldIds</c>): the file names in
/// every <c>adr/</c> folder for <c>adr</c> and in every <c>lessons/</c> folder for <c>lesson</c> (<c>0001-F19</c> B-038,
/// B-039), listed through the check's own discovery and its exclusions, and never written (C-1, C-11). The root
/// <c>.spec/</c>, which the default exclusions skip as no specification, is read: it is where the repository-wide records live.
/// </summary>
/// <param name="discovery">The listing of the tree.</param>
/// <implements epic="0001" feature="F19" claim="B-038" />
/// <implements epic="0001" feature="F19" claim="B-039" />
public sealed class HeldIds(ISpecDiscovery discovery)
{
    /// <summary>
    /// Lists the name of each file in the folders a type's ids are held in, outside the manifest's exclusions less the root
    /// <c>.spec/</c> skip.
    /// </summary>
    /// <param name="type">The id type.</param>
    /// <param name="root">The repository root.</param>
    /// <param name="exclusions">The manifest's directory exclusions.</param>
    /// <returns>The file names, none for a type whose places are not read.</returns>
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    public IReadOnlyList<string> Of(string type, string root, IReadOnlyList<string> exclusions)
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

    /// <summary>The exclusion that skips the root <c>.spec/</c> (<c>0001-F6</c> decision 0003), which the guard does not apply.</summary>
    /// <implements epic="0001" feature="F19" claim="B-038" />
    /// <implements epic="0001" feature="F19" claim="B-039" />
    private const string RootSpec = "/.spec";
}
