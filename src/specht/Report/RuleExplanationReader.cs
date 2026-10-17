using Specht.Manifest;
using Specht.Model;

namespace Specht.Report;

/// <summary>Reads the pinned rule page after manifest validation, without loading a tree (0001-F3 B-019 to B-020, B-029 to B-030).</summary>
/// <param name="schemas">The existing manifest reader.</param>
/// <param name="parser">The parser that validates the exact schema pin and its vocabulary.</param>
public sealed class RuleExplanationReader(SpecSchemasLoader schemas, SpecManifestParser parser)
{
    /// <summary>Returns the entire embedded page, including frontmatter, or null for an id outside the pinned vocabulary.</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="id">The exact rule id.</param>
    /// <returns>The authoritative page without rendering or newline changes, or null for an unknown rule.</returns>
    public string? Read(string root, string id)
    {
        var version = parser.PinnedVersion(schemas.LoadManifest(root));
        return version.RuleIds.Contains(id) ? version.RulePages[id] : null;
    }
}
