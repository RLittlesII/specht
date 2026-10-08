namespace specht;

/// <summary>A claim tag found in a Gherkin file.</summary>
/// <param name="Id">The claim id, e.g. <c>B-001</c>.</param>
/// <param name="Line">One-based line the tag sits on.</param>
public sealed record FeatureTag(string Id, int Line);
