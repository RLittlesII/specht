using System.Collections.Generic;

namespace Specht.Analysis.Bindings.Gherkin;

public sealed record GherkinClaimFile(
    string RelativePath,
    IReadOnlyList<BindingClaim> Claims,
    IReadOnlyList<BindingFinding> Findings,
    IReadOnlyList<string> FeatureTagValues,
    IReadOnlyList<string> EpicTagValues);
