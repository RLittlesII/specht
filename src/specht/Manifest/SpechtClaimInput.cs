namespace Specht.Manifest;

/// <summary>The <c>claim</c> members a manifest pinned to schema 0.4.0 or later selects claim input with.</summary>
/// <param name="Source">Where claims come from, read from <c>claim.source</c>.</param>
/// <param name="Files">The claim file sets, read from <c>claim.files</c>; empty under <see cref="SpechtClaimSource.Markdown"/>.</param>
/// <param name="PlannedTags">The tags that mark a scenario planned, read from <c>claim.plannedTags</c>.</param>
/// <param name="WithdrawnTags">The tags that withdraw a claim, read from <c>claim.withdrawnTags</c>.</param>
/// <param name="SupersededTags">The tags that supersede a claim, read from <c>claim.supersededTags</c>.</param>
/// <param name="LifecycleKey">The record frontmatter key that holds a Feature's lifecycle, read from <c>claim.lifecycleKey</c>.</param>
/// <param name="FeatureScopeTag">The Feature-level tag form, read from <c>claim.scopes.featureTag</c>; null when not declared.</param>
/// <param name="EpicScopeTag">The epic-level tag form, read from <c>claim.scopes.epicTag</c>; null when not declared.</param>
/// <implements epic="0001" feature="F16" claim="B-002" />
/// <implements epic="0001" feature="F16" claim="B-029" />
public sealed record SpechtClaimInput(
    SpechtClaimSource Source,
    IReadOnlyList<SpechtClaimFileSet> Files,
    IReadOnlyList<string> PlannedTags,
    IReadOnlyList<string> WithdrawnTags,
    IReadOnlyList<string> SupersededTags,
    string LifecycleKey,
    string? FeatureScopeTag,
    string? EpicScopeTag);
