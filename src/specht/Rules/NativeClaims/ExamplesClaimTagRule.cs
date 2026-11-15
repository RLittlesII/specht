using Specht.Model;

namespace Specht.Rules.NativeClaims;

/// <summary>
/// SPEC026 reports, at its claim file's path and the tag's line, each tag on an Examples block that the shared Gherkin reader
/// finds the claim tag form reading, naming the claim id it would declare, once per claim file however many sets match it.
/// </summary>
/// <implements epic="0001" feature="F16" claim="B-009" />
public sealed class ExamplesClaimTagRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC026";

    /// <inheritdoc />
    /// <implements epic="0001" feature="F16" claim="B-009" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model) =>
        model.ClaimSets
            .SelectMany(static claims => claims.Files)
            .DistinctBy(static file => file.RelativePath, StringComparer.Ordinal)
            .SelectMany(static file => file.Findings
                .Where(static finding => finding.DiagnosticId == "SPCT0011")
                .Select(finding => new SpechtFinding(file.RelativePath, finding.Line, finding.ClaimId,
                    "claim tag on an Examples block declares no claim")));
}
