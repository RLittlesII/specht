using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>A Feature's claim rows and valid ids, derived once for claim and companion rules.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpechtClaims
{
    /// <summary>Derives the rows under the manifest's claims role, retaining duplicates and malformed ids.</summary>
    /// <param name="document">The Feature document.</param>
    /// <param name="structure">The effective manifest.</param>
    /// <param name="grammar">The claim grammar shared across this model build.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpechtClaims(SpechtDocument document, SpechtStructure structure, Regex? grammar)
    {
        Section = document.Section(structure.Roles["claims"]);
        var rows = new List<SpechtClaim>();
        var ids = new Dictionary<string, int>(StringComparer.Ordinal);
        var tagIds = new HashSet<string>(structure.ClaimTagIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        if (Section is not null)
        {
            for (var index = 0; index < Section.Rows.Count; index++)
            {
                var row = Section.Rows[index];
                var id = row.Count == 0 ? string.Empty : row[0].Trim();
                var valid = grammar?.IsMatch(id) == true;
                var first = valid && id.Length > 0 && ids.TryGetValue(id, out var previous) ? previous : (int?) null;
                rows.Add(new SpechtClaim(id, Section.RowLines[index], valid, first));
                if (valid && id.Length > 0)
                {
                    ids.TryAdd(id, Section.RowLines[index]);
                    tagIds.Add(id);
                }
            }
        }

        Rows = rows.AsReadOnly();
        Ids = new ReadOnlyDictionary<string, int>(ids);
        TagIds = new ReadOnlySet<string>(tagIds);
    }

    /// <summary>
    /// Derives a native Feature's claims from its claim file set: each claim its files declare, in file order and then
    /// document order, located at the keyword line of the element that declares it, except the claims of a file in
    /// <paramref name="unclaimed"/>, which declares none.
    /// </summary>
    /// <param name="set">The claim file set.</param>
    /// <param name="files">What the shared Gherkin reader read from each of the set's files, in listing order.</param>
    /// <param name="structure">The effective manifest.</param>
    /// <param name="unclaimed">The root-relative paths of the files whose claims are under no set, or null for none.</param>
    /// <implements epic="0001" feature="F16" claim="B-001" />
    /// <implements epic="0001" feature="F16" claim="B-004" />
    /// <implements epic="0001" feature="F16" claim="B-011" />
    /// <implements epic="0001" feature="F16" claim="B-010" />
    /// <implements epic="0001" feature="F16" claim="B-018" />
    public SpechtClaims(SpechtClaimFileSet set, IReadOnlyList<GherkinClaimText> files, SpechtStructure structure, IReadOnlySet<string>? unclaimed = null)
    {
        FileSet = set;
        Files = files;
        var rows = new List<SpechtClaim>();
        var ids = new Dictionary<string, int>(StringComparer.Ordinal);
        var tagIds = new HashSet<string>(structure.ClaimTagIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        foreach (var file in files.Where(file => unclaimed?.Contains(file.RelativePath) != true))
        {
            foreach (var declaration in file.Claims)
            {
                rows.Add(new SpechtClaim(declaration.Id, declaration.Line, true, null)
                {
                    Path = file.RelativePath,
                    Declaration = declaration,
                });
                ids.TryAdd(declaration.Id, declaration.Line);
                tagIds.Add(declaration.Id);
            }
        }

        Rows = rows.AsReadOnly();
        Ids = new ReadOnlyDictionary<string, int>(ids);
        TagIds = new ReadOnlySet<string>(tagIds);
    }

    /// <summary>The claim file set a native Feature's claims are read from; null for a claims table.</summary>
    /// <implements epic="0001" feature="F16" claim="B-001" />
    public SpechtClaimFileSet? FileSet { get; }

    /// <summary>What the shared Gherkin reader read from each claim file of the set, in listing order; empty for a claims table.</summary>
    /// <implements epic="0001" feature="F16" claim="B-010" />
    public IReadOnlyList<GherkinClaimText> Files { get; } = [];

    /// <summary>
    /// The claim input of the set the check could not take claims from: each claim file the reader refused, the set's glob
    /// when it matches no file, each file another set also matches, and each Feature-level tag naming another Feature; empty
    /// for a claims table.
    /// </summary>
    /// <implements epic="0001" feature="F16" claim="B-014" />
    /// <implements epic="0001" feature="F16" claim="B-015" />
    /// <implements epic="0001" feature="F16" claim="B-016" />
    /// <implements epic="0001" feature="F16" claim="B-018" />
    /// <implements epic="0001" feature="F16" claim="B-019" />
    public IReadOnlyList<SpechtClaimFault> Faults { get; init; } = [];

    /// <summary>The claims section, absent when no section has the configured role.</summary>
    public SpechtSection? Section { get; }

    /// <summary>Every source row in order.</summary>
    public IReadOnlyList<SpechtClaim> Rows { get; }

    /// <summary>Each valid id and its first declaration line, compared ordinally.</summary>
    public IReadOnlyDictionary<string, int> Ids { get; }

    /// <summary>Valid ids under the configured tag-case comparison.</summary>
    public IReadOnlySet<string> TagIds { get; }
}
