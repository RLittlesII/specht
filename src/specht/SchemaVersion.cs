namespace specht;

/// <summary>One schema version the tool ships: its three frontmatter schemas and its rule vocabulary (<c>0001-F7</c>).</summary>
/// <param name="Number">The version a manifest pins with <c>schemaVersion</c>.</param>
/// <param name="FeatureSchema">The Feature specification frontmatter schema's text.</param>
/// <param name="ItemSchema">The task, test, bug and spike frontmatter schema's text.</param>
/// <param name="EpicSchema">The epic frontmatter schema's text.</param>
/// <param name="RuleIds">Every <c>SPEC###</c> id a check under this version evaluates (B-014).</param>
public sealed record SchemaVersion(int Number, string FeatureSchema, string ItemSchema, string EpicSchema, IReadOnlySet<string> RuleIds);
