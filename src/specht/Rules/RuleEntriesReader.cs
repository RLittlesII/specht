using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>Reads rule entries from the validated pin, its pages and explicit manifest levels (0001-F10 B-001 to B-008).</summary>
/// <param name="schemas">The existing root and manifest loader.</param>
/// <param name="parser">The existing exact-pin resolver.</param>
/// <implements epic="0001" feature="F10" claim="B-001" />
public sealed class RuleEntriesReader(SpecSchemasLoader schemas, SpecManifestParser parser)
{
    /// <summary>Reads only the root's validated manifest before producing its rule entries.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The pinned vocabulary's entries in ordinal id order.</returns>
    /// <implements epic="0001" feature="F10" claim="B-001" />
    public IReadOnlyList<RuleEntry> Read(string root) => Read(schemas.LoadManifest(root));

    /// <summary>Produces rule entries from an already parsed manifest without file-system access.</summary>
    /// <param name="structure">The manifest produced by the check's parser.</param>
    /// <returns>The pinned vocabulary's entries in ordinal id order.</returns>
    /// <implements epic="0001" feature="F10" claim="B-001" />
    /// <implements epic="0001" feature="F10" claim="B-002" />
    /// <implements epic="0001" feature="F10" claim="B-003" />
    /// <implements epic="0001" feature="F10" claim="B-004" />
    /// <implements epic="0001" feature="F10" claim="B-005" />
    /// <implements epic="0001" feature="F10" claim="B-006" />
    /// <implements epic="0001" feature="F10" claim="B-007" />
    /// <implements epic="0001" feature="F10" claim="B-008" />
    public IReadOnlyList<RuleEntry> Read(SpecStructure structure)
    {
        var version = parser.PinnedVersion(structure);
        return version.RuleIds.Order(StringComparer.Ordinal).Select(id =>
        {
            var page = SpecDocument.Parse(version.RulePages[id], $"rules/v{version.Number}/{id}.md");
            var metadata = page.Section("Metadata")!.Rows;
            var family = metadata.Single(static row => row[0] == "Family")[1];
            var severity = Enum.Parse<SpecRuleLevel>(metadata.Single(static row => row[0] == "Default severity")[1], ignoreCase: true);
            var effective = structure.Rules.TryGetValue(id, out var level) ? level : severity;
            return new RuleEntry(id, family, severity, effective);
        }).ToArray();
    }
}
