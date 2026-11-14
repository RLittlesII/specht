using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules.NativeClaims;

/// <summary>
/// SPEC025 reports, at its claim file's path and keyword line, each Scenario or Scenario Outline the shared Gherkin reader
/// finds carrying no claim of its own, its Rule's or its Feature's, once per claim file however many sets match it.
/// </summary>
/// <implements epic="0001" feature="F16" claim="B-010" />
public sealed class ScenarioWithoutClaimRule : ISpechtRule
{
    /// <inheritdoc />
    public string Id => "SPEC025";

    /// <inheritdoc />
    public SpechtRuleLevel DefaultLevel => SpechtRuleLevel.Warning;

    /// <inheritdoc />
    /// <implements epic="0001" feature="F16" claim="B-010" />
    public IEnumerable<SpechtFinding> Evaluate(SpechtModel model) =>
        model.ClaimSets
            .SelectMany(static claims => claims.Files)
            .DistinctBy(static file => file.RelativePath, StringComparer.Ordinal)
            .SelectMany(static file => file.Findings
                .Where(static finding => finding.DiagnosticId == "SPCT0012")
                .Select(finding => new SpechtFinding(file.RelativePath, finding.Line, null,
                    "scenario declares no claim and inherits none from its Rule or its Feature")));
}
