namespace Specht.Facts;

/// <summary>
/// One discovered Feature specification's facts (<c>0132-F2</c> B-003, B-004): every member is present, with <c>null</c>
/// or an empty list when the frontmatter has no value, so no member's presence depends on the tree or the pin (B-017).
/// </summary>
/// <param name="Path">The specification's path, relative to the root with <c>/</c> separators.</param>
/// <param name="Id">The qualified Feature id the check reads, such as <c>0132-F2</c>, or <c>null</c> without an identity.</param>
/// <param name="SpecStatus">The specification's maturity, read through the manifest's key, or <c>null</c>.</param>
/// <param name="Status">The delivery status, read through the manifest's key, or <c>null</c>.</param>
/// <param name="DependsOn">The depends-on list as the frontmatter writes it, unresolved.</param>
/// <param name="Blocks">The blocks list as the frontmatter writes it, unresolved.</param>
/// <param name="Children">The children list as the frontmatter writes it.</param>
/// <param name="Claims">Every row of the claims table, in row order; empty without a claims section or table.</param>
/// <param name="Constraints">Every row of the constraints table, in row order; empty without a constraints section or table.</param>
/// <param name="OutOfScope">Every row of the out-of-scope table, in row order; empty without an out-of-scope section or table.</param>
/// <param name="OpenQuestions">Every row of the open-questions table, in row order; empty without an open-questions section or table.</param>
/// <param name="Records">The records beside the specification, in ordinal order of path; empty when it has none.</param>
/// <implements epic="0132" feature="F2" claim="B-003" />
/// <implements epic="0132" feature="F2" claim="B-004" />
/// <implements epic="0132" feature="F2" claim="B-005" />
/// <implements epic="0132" feature="F2" claim="B-006" />
/// <implements epic="0132" feature="F2" claim="B-007" />
/// <implements epic="0132" feature="F2" claim="B-008" />
/// <implements epic="0132" feature="F2" claim="B-011" />
/// <implements epic="0132" feature="F2" claim="B-015" />
public sealed record FeatureFacts(
    string Path,
    string? Id,
    string? SpecStatus,
    string? Status,
    IReadOnlyList<string> DependsOn,
    IReadOnlyList<string> Blocks,
    IReadOnlyList<string> Children,
    IReadOnlyList<ClaimFacts> Claims,
    IReadOnlyList<ConstraintFacts> Constraints,
    IReadOnlyList<OutOfScopeFacts> OutOfScope,
    IReadOnlyList<OpenQuestionFacts> OpenQuestions,
    IReadOnlyList<RecordFacts> Records);
