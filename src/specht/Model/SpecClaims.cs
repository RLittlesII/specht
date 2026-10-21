using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Specht.Manifest;

namespace Specht.Model;

/// <summary>A Feature's claim rows and valid ids, derived once for claim and companion rules.</summary>
/// <implements epic="0001" feature="F1" claim="B-004" />
public sealed class SpecClaims
{
    /// <summary>Derives the rows under the manifest's claims role, retaining duplicates and malformed ids.</summary>
    /// <param name="document">The Feature document.</param>
    /// <param name="structure">The effective manifest.</param>
    /// <param name="grammar">The claim grammar shared across this model build.</param>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public SpecClaims(SpecDocument document, SpecStructure structure, Regex? grammar)
    {
        Section = document.Section(structure.Roles["claims"]);
        var rows = new List<SpecClaim>();
        var ids = new Dictionary<string, int>(StringComparer.Ordinal);
        var tagIds = new HashSet<string>(structure.ClaimTagIgnoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        if (Section is not null)
        {
            for (var index = 0; index < Section.Rows.Count; index++)
            {
                var row = Section.Rows[index];
                var id = row.Count == 0 ? string.Empty : row[0].Trim();
                var valid = grammar?.IsMatch(id) == true;
                var first = valid && id.Length > 0 && ids.TryGetValue(id, out var previous) ? previous : (int?)null;
                rows.Add(new SpecClaim(id, Section.RowLines[index], valid, first));
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

    /// <summary>The claims section, absent when no section has the configured role.</summary>
    public SpecSection? Section { get; }

    /// <summary>Every source row in order.</summary>
    public IReadOnlyList<SpecClaim> Rows { get; }

    /// <summary>Each valid id and its first declaration line, compared ordinally.</summary>
    public IReadOnlyDictionary<string, int> Ids { get; }

    /// <summary>Valid ids under the configured tag-case comparison.</summary>
    public IReadOnlySet<string> TagIds { get; }
}
