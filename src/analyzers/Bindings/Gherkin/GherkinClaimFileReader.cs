using System;
using System.Linq;
using Specht.Model;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>Reads claim declarations, cases and findings from English Gherkin text, line by line, with no I/O.</summary>
/// <implements epic="0201" feature="F7" claim="B-008" />
public sealed class GherkinClaimFileReader
{
    public GherkinClaimFileReader(GherkinClaimPolicy policy)
    {
        Policy = policy;
        _reader = new GherkinClaimTextReader(new GherkinClaimTags(policy.Claim.TagForm, policy.FeatureScope?.TagForm,
            policy.EpicScope?.TagForm, policy.PlannedTags, policy.WithdrawnTags, policy.SupersededTags));
    }

    public GherkinClaimPolicy Policy { get; }

    public GherkinClaimFile Read(string relativePath, string text)
    {
        GherkinClaimText read;
        try
        {
            read = _reader.Read(relativePath, text);
        }
        catch (GherkinClaimTextException failure)
        {
            throw new GherkinInputException(failure.RelativePath, failure.Message);
        }

        return new GherkinClaimFile(read.RelativePath,
            Array.AsReadOnly(read.Claims.Select(claim => Claim(read.RelativePath, claim)).ToArray()),
            Array.AsReadOnly(read.Findings.Select(finding => new BindingFinding(finding.DiagnosticId, read.RelativePath, finding.Line,
                finding.Text)).ToArray()),
            read.FeatureTagValues, read.EpicTagValues);
    }

    private static BindingClaim Claim(string path, GherkinClaimDeclaration claim) =>
        new(claim.Id, claim.State, claim.Line, RelativePath: path, Element: Element(claim.Element),
            Cases: Array.AsReadOnly(claim.Cases.Select(static item => new BindingClaimCase(item.Line, item.Planned)).ToArray()));

    private static BindingClaimElement Element(GherkinClaimElement element) => element switch
    {
        GherkinClaimElement.Feature => BindingClaimElement.Feature,
        GherkinClaimElement.Rule => BindingClaimElement.Rule,
        GherkinClaimElement.Scenario => BindingClaimElement.Scenario,
        _ => BindingClaimElement.ScenarioOutline,
    };

    private readonly GherkinClaimTextReader _reader;
}
