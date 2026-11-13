namespace Specht.Manifest;

/// <summary>Where a manifest pinned to schema 0.4.0 or later says claims come from, read from <c>claim.source</c>.</summary>
/// <implements epic="0001" feature="F16" claim="B-002" />
public enum SpechtClaimSource
{
    /// <summary>The claims table of each specification, as below schema 0.4.0.</summary>
    Markdown,

    /// <summary>The claim tags of the <c>.feature</c> files each claim file set names.</summary>
    Gherkin,
}
