using Specht.Discovery;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>The ordered section contract and the id grammars, read from <c>.specht/manifest.json</c>.</summary>
/// <param name="Sections">The <c>## </c> headings a specification must carry, in order.</param>
/// <param name="Tables">Expected header cells, keyed by role.</param>
/// <param name="Identifiers">Id grammars, keyed by name.</param>
/// <param name="SchemaVersion">The schema version the manifest pins, <c>0.1.0</c> when it pins none (<c>0001-F7</c> B-002).</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed record SpecStructure(
    IReadOnlyList<string> Sections,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Tables,
    IReadOnlyDictionary<string, string> Identifiers,
    SemanticVersion SchemaVersion)
{
    /// <summary>What discovery finds and skips (<c>0001-F6</c> B-001, B-002, B-003, decision 0004).</summary>
    public SpecDiscoveryInputs Discovery { get; init; } = new([], [], [], [], []);

    /// <summary>The frontmatter schema file names under <c>.specht/schema/frontmatter/</c>, keyed by kind (B-008, decision 0003).</summary>
    public IReadOnlyDictionary<string, string> FrontmatterSchemas { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The section title each role names, keyed by role (B-001, decision 0006).</summary>
    public IReadOnlyDictionary<string, string> Roles { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The text each marker is written as, keyed by marker (B-003, decision 0006).</summary>
    public IReadOnlyDictionary<string, string> Markers { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The level each rule id is set to, <c>error</c>, <c>warning</c> or <c>off</c>, keyed by rule id (B-010, B-011, decision 0008).</summary>
    public SpecRuleSettings Rules { get; init; } = new([]);

    /// <summary>The literal claim-tag form at schema 0.2.0 and later; null selects the legacy scanner.</summary>
    public string? ClaimTagForm { get; init; }

    /// <summary>Whether claim tags, including literal form text, ignore letter case (B-037).</summary>
    public bool ClaimTagIgnoreCase { get; init; }

    /// <summary>The explicit frontmatter schema source, null when the manifest leaves selection automatic.</summary>
    public SchemaSourceSelection? SchemaSource { get; init; }

    /// <summary>Whether the original manifest declares an epic grammar before omitted identifiers are filled.</summary>
    public bool DeclaresEpicGrammar { get; init; }

    /// <summary>The upstream schema source the manifest records, null when it records none (<c>0001-F7</c> B-068).</summary>
    public UpstreamSchemaSource? SpechtSchema { get; init; }

    /// <summary>The test files and declaration forms at schema 0.2.0 and later; null when the manifest declares none (B-055).</summary>
    public SpecTestSettings? Tests { get; init; }

    /// <summary>Whether matrix resolution also matches scenario titles; false unless the manifest turns it on (B-057).</summary>
    public bool MatrixScenarioTitles { get; init; }

    /// <summary>The roles that own each <c>sections</c> title, as the manifest wrote them; empty when it names none (B-059).</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Owners { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

    /// <summary>The directory globs that must hold a specification; empty when the manifest names none (B-060).</summary>
    public IReadOnlyList<string> RequiredSpecifications { get; init; } = [];

    /// <summary>The template file names the manifest names as the consumer's; empty when it names none, and no rule reads it (<c>0001-F7</c> B-065).</summary>
    public IReadOnlyList<string> ConsumerTemplates { get; init; } = [];
}
