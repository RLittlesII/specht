namespace Specht.Manifest;

/// <summary>One entry of <c>claim.files</c>: the claim files of one Feature identity.</summary>
/// <param name="Glob">The glob that selects the set's <c>.feature</c> files, as the manifest wrote it.</param>
/// <param name="Feature">The Feature the set names.</param>
/// <param name="Epic">The epic the set names, null when the entry has none.</param>
/// <param name="Record">The path of the set's record document, null when the entry has none.</param>
/// <implements epic="0001" feature="F16" claim="B-027" />
/// <implements epic="0001" feature="F16" claim="B-029" />
public sealed record SpechtClaimFileSet(string Glob, string Feature, string? Epic, string? Record);
