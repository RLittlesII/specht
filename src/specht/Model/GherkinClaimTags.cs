using System.Collections.Generic;

namespace Specht.Model;

/// <summary>The tag forms and tag lists the Gherkin claim reader reads a claim file with.</summary>
/// <implements epic="0201" feature="F7" claim="B-004" />
public sealed record GherkinClaimTags(
    GherkinClaimTagForm Claim,
    GherkinClaimTagForm? FeatureScope,
    GherkinClaimTagForm? EpicScope,
    IReadOnlyList<string> PlannedTags,
    IReadOnlyList<string> WithdrawnTags,
    IReadOnlyList<string> SupersededTags);
