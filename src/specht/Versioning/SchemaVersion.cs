namespace Specht.Versioning;

/// <summary>
/// One schema version the tool ships: its three frontmatter schemas, its rule vocabulary (<c>0001-F7</c>) and its rule
/// pages (<c>0001-F3</c>).
/// </summary>
/// <param name="Number">The version a manifest pins with <c>schemaVersion</c>.</param>
/// <param name="FeatureSchema">The Feature specification frontmatter schema's text.</param>
/// <param name="ItemSchema">The task, test, bug and spike frontmatter schema's text.</param>
/// <param name="EpicSchema">The epic frontmatter schema's text.</param>
/// <param name="RuleIds">Every rule id a check under this version evaluates, in this version's form (B-014, B-151, B-152).</param>
/// <param name="Manifest">The version's shipped manifest text supplying omission defaults (0001-F5 B-048).</param>
/// <implements epic="0001" feature="F7" claim="B-001" />
public sealed record SchemaVersion(
    SemanticVersion Number, string FeatureSchema, string ItemSchema, string EpicSchema, IReadOnlySet<string> RuleIds, string Manifest)
{
    /// <summary>Gets each rule page this version ships, its Markdown text keyed by rule id (<c>0001-F3</c> B-031, C-6).</summary>
    public IReadOnlyDictionary<string, string> RulePages { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>Gets the file name of each template this version ships (<c>0001-F7</c> B-127).</summary>
    public IReadOnlySet<string> Templates { get; init; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the id this version names a rule by, keyed by the id the rule states; a rule it does
    /// not hold keeps its own id (<c>0001-F7</c> B-151, B-152; ADR-0029).
    /// </summary>
    public IReadOnlyDictionary<string, string> RuleNames { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The id this version names the rule <paramref name="ruleId"/> by (<c>0001-F7</c> B-151, B-152).</summary>
    /// <param name="ruleId">A rule's own id.</param>
    /// <returns>This version's id for it.</returns>
    /// <implements epic="0001" feature="F7" claim="B-151" />
    /// <implements epic="0001" feature="F7" claim="B-152" />
    public string RuleId(string ruleId) => RuleNames.TryGetValue(ruleId, out var id) ? id : ruleId;
}
