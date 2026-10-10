namespace specht;

/// <summary>The ordered section contract and the id grammars, read from <c>.spec/schema/</c>.</summary>
/// <param name="Sections">The <c>## </c> headings a specification must carry, in order.</param>
/// <param name="Tables">Expected header cells, keyed by section title.</param>
/// <param name="Identifiers">Id grammars, keyed by name.</param>
/// <param name="SchemaVersion">The schema version the manifest pins, 1 when it pins none (<c>0001-F7</c> B-002).</param>
public sealed record SpecStructure(
    IReadOnlyList<string> Sections,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Tables,
    IReadOnlyDictionary<string, string> Identifiers,
    int SchemaVersion);
