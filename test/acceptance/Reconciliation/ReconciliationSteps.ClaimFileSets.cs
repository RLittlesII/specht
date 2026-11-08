using System.Threading;
using AwesomeAssertions;
using Reqnroll;
using Specht.Analysis.Policy;

namespace Specht.Acceptance.Reconciliation;

public sealed partial class ReconciliationSteps
{
    [Given("a claim file set whose glob is spelled {string}")]
    public void GivenSpelledClaimFileSet(string spelling)
    {
        _tree.UseNativeClaims(BoundClaimFile);
        _tree.SpellClaimFileSet(spelling switch
        {
            "with a leading ./" => "./.spec/features/read/*.feature",
            "with an empty segment" => ".spec//features/read/*.feature",
            "with a backslash" => string.Join('\\', ".spec", "features", "read", "*.feature"),
            "with .. inside the root" => ".spec/other/../features/read/*.feature",
            "ending in **" => ".spec/features/read/**",
            _ => throw new ArgumentOutOfRangeException(nameof(spelling), spelling, null),
        }, new Dictionary<string, string>
        {
            [".spec/features/read/read.feature"] = "Feature: Read\n\n  @need/8\n  Scenario: Reads a listed value\n    Then it is read\n",
            [".spec/features/read/more/more.feature"] = "Feature: Read more\n\n  @need/9\n  Scenario: Reads a nested value\n    Then it is read\n",
        });
    }

    [When("the analyzer builds its native claim catalogue and specht evidence reconciles the supplied evidence")]
    public void WhenBothReadTheClaimFileSet()
    {
        _catalogue = new AnalysisInputReader().Read(_tree.AnalyzerInputs(), CancellationToken.None);
        Run(true);
    }

    [Then("both read the same claim files from that set")]
    public void ThenBothReadTheSameClaimFiles()
    {
        _catalogue!.Failures.Should().BeEmpty();
        var analyzer = _catalogue.Catalog!.Features.SelectMany(static feature => feature.Claims)
            .Select(static claim => claim.RelativePath!).Distinct().Order(StringComparer.Ordinal).ToArray();
        var evidence = Report()["claims"]!.AsArray().Select(static claim => claim!["path"]!.GetValue<string>())
            .Distinct().Order(StringComparer.Ordinal).ToArray();
        analyzer.Should().NotBeEmpty();
        evidence.Should().Equal(analyzer);
    }

    private AnalysisSnapshot? _catalogue;
}
