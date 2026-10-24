using System.Collections.Generic;

namespace Specht.Analysis.Bindings.Gherkin;

public sealed record GherkinClaimPolicy(
    GherkinTagForm Claim,
    GherkinTagForm? FeatureScope,
    GherkinTagForm? EpicScope,
    IReadOnlyList<string> PlannedTags,
    IReadOnlyList<string> WithdrawnTags,
    IReadOnlyList<string> SupersededTags,
    IReadOnlyList<GherkinClaimFileSet> FileSets);
