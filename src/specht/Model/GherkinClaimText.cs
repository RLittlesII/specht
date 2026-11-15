using System.Collections.Generic;

namespace Specht.Model;

/// <summary>
/// The claims, findings and Feature and epic tag values read from one Gherkin claim file, with the line of each Feature
/// tag value (<c>0001-F16</c> B-019).
/// </summary>
/// <implements epic="0201" feature="F7" claim="B-008" />
public sealed record GherkinClaimText(
    string RelativePath,
    IReadOnlyList<GherkinClaimDeclaration> Claims,
    IReadOnlyList<GherkinClaimTextFinding> Findings,
    IReadOnlyList<string> FeatureTagValues,
    IReadOnlyList<string> EpicTagValues,
    IReadOnlyList<int>? FeatureTagLines = null,
    IReadOnlyList<(int Line, string Title)>? Scenarios = null);
