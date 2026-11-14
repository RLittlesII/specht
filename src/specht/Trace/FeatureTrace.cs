namespace Specht.Trace;

/// <summary>One Feature specification's trace (<c>0001-F13</c> B-003).</summary>
/// <param name="Path">The specification's path, relative to the root with <c>/</c> separators.</param>
/// <param name="Id">The qualified Feature id the check reads, or <c>null</c> without an identity.</param>
/// <param name="Claims">Every claims-table row, in row order.</param>
/// <implements epic="0001" feature="F13" claim="B-003" />
public sealed record FeatureTrace(string Path, string? Id, IReadOnlyList<ClaimTrace> Claims);
