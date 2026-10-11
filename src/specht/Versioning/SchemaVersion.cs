namespace Specht.Versioning;

/// <summary>
/// One schema version the tool ships: its three frontmatter schemas, its rule vocabulary (<c>0001-F7</c>) and its rule
/// pages (<c>0001-F3</c>).
/// </summary>
/// <param name="Number">The version a manifest pins with <c>schemaVersion</c>.</param>
/// <param name="FeatureSchema">The Feature specification frontmatter schema's text.</param>
/// <param name="ItemSchema">The task, test, bug and spike frontmatter schema's text.</param>
/// <param name="EpicSchema">The epic frontmatter schema's text.</param>
/// <param name="RuleIds">Every <c>SPEC###</c> id a check under this version evaluates (B-014).</param>
public sealed record SchemaVersion(SemanticVersion Number, string FeatureSchema, string ItemSchema, string EpicSchema, IReadOnlySet<string> RuleIds)
{
    /// <summary>Gets each rule page this version ships, its Markdown text keyed by rule id (<c>0001-F3</c> B-031, C-6).</summary>
    public IReadOnlyDictionary<string, string> RulePages { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}
