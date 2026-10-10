using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests;
using Specht.Tests.Rules;

namespace Specht.Acceptance.Report;

/// <summary>
/// Steps for the rule page scenarios of <c>src/specht/Report/.spec/report.feature</c> (0001-F3 B-031, B-032, B-033; item
/// 0129). The pages are the ones every embedded schema version gives through <see cref="SchemaVersion.RulePages"/>, the
/// seam <c>--explain</c> reads (item 0040); "a schema version the tool ships" is each of them in turn. A scenario over no
/// page proves nothing, so the steps that gather pages fail when a version gives none.
/// </summary>
[Binding]
[Scope(Feature = "The report contract")]
public sealed class RulePageSteps
{
    [Given("a schema version the tool ships")]
    public void GivenASchemaVersionTheToolShips() => _versions = SchemaVersions.Embedded.Versions;

    [Given("the rule pages for a schema version the tool ships")]
    public void GivenTheRulePagesForASchemaVersionTheToolShips()
    {
        GivenASchemaVersionTheToolShips();
        WhenTheRulePagesForThatVersionAreGathered();
    }

    [When("the rule pages for that version are gathered")]
    public void WhenTheRulePagesForThatVersionAreGathered() =>
        _versions.Should().NotBeEmpty().And.AllSatisfy(static version =>
            version.RulePages.Should().NotBeEmpty("version {0} has {1} rules to page", version.Number, version.RuleIds.Count));

    [When("each page is read")]
    public void WhenEachPageIsRead() =>
        _pages = _versions
            .SelectMany(static version => version.RulePages.Select(page => RulePage.Read(EmbeddedFolder.Of(version), page.Key, page.Value)))
            .ToList();

    [Then("every rule in that version's vocabulary has exactly one page")]
    public void ThenEveryRuleInThatVersionsVocabularyHasExactlyOnePage() =>
        _versions.Should().AllSatisfy(static version => version.RuleIds.Should().AllSatisfy(id =>
            version.RulePages.Keys.Where(page => string.Equals(page, id, StringComparison.Ordinal)).Should().ContainSingle()));

    [Then("no page is for a rule outside that version's vocabulary")]
    public void ThenNoPageIsForARuleOutsideThatVersionsVocabulary() =>
        _versions.Should().AllSatisfy(static version => version.RulePages.Keys.Should().BeSubsetOf(version.RuleIds));

    [Then("it declares itself a rule page, with a title and a description")]
    public void ThenItDeclaresItselfARulePageWithATitleAndADescription() =>
        _pages.Should().AllSatisfy(static page =>
        {
            page.Frontmatter.Text("type").Should().Be("rule");
            page.Frontmatter.Text("title").Should().NotBeNullOrWhiteSpace();
            page.Frontmatter.Text("description").Should().NotBeNullOrWhiteSpace();
        });

    [Then("its heading names the rule it is for, then the rule's title")]
    public void ThenItsHeadingNamesTheRuleItIsForThenTheRulesTitle() =>
        _pages.Should().AllSatisfy(static page =>
            page.Outline.FirstOrDefault().Should().StartWith($"# {page.Id}: ").And.NotBe($"# {page.Id}: "));

    [Then("it gives the rule's id, its family, its default severity and its schema version")]
    public void ThenItGivesTheRulesIdItsFamilyItsDefaultSeverityAndItsSchemaVersion() =>
        _pages.Should().AllSatisfy(static page =>
        {
            page.Value("Rule ID").Should().Be(page.Id);
            page.Value("Family").Should().NotBeNullOrWhiteSpace();
            page.Value("Default severity").Should().NotBeNullOrWhiteSpace();
            page.Value("Schema version").Should().NotBeNullOrWhiteSpace();
        });

    [Then("it says what the rule checks")]
    public void ThenItSaysWhatTheRuleChecks() =>
        _pages.Should().AllSatisfy(static page => page.Body("## Cause").Should().NotBeEmpty());

    [Then("it describes what the rule expects, with an example violation, the line the tool prints for it, and the corrected example")]
    public void ThenItDescribesWhatTheRuleExpectsWithAnExampleViolationTheLineTheToolPrintsForItAndTheCorrectedExample() =>
        _pages.Should().AllSatisfy(static page =>
        {
            page.Body("## Rule description").Should().NotBeEmpty();
            page.Body("### Example violation").Should().NotBeEmpty();
            page.ToolLineFiles().Should().NotBeEmpty();
            page.Body("### Corrected").Should().NotBeEmpty();
        });

    [Then("it says how to fix a violation")]
    public void ThenItSaysHowToFixAViolation() =>
        _pages.Should().AllSatisfy(static page => page.Body("## How to fix violations").Should().NotBeEmpty());

    [Then("those parts come in that order")]
    public void ThenThosePartsComeInThatOrder() =>
        _pages.Should().AllSatisfy(static page =>
        {
            page.Text.Should().StartWith("---");
            page.Outline.Skip(1).Intersect(RulePage.Shape).Should().Equal(RulePage.Shape);
        });

    [Then("it has no part beyond the one page shape")]
    public void ThenItHasNoPartBeyondTheOnePageShape() =>
        _pages.Should().AllSatisfy(static page => page.Outline.Skip(1).Should().Equal(RulePage.Shape));

    [Then("it says nothing on suppressing, disabling or changing the severity of the rule")]
    public void ThenItSaysNothingOnSuppressingDisablingOrChangingTheSeverityOfTheRule() =>
        _pages.Should().AllSatisfy(static page =>
        {
            Regex.Count(page.Text, "suppress|disabl", RegexOptions.IgnoreCase).Should().Be(0);
            Regex.Count(page.Text, "severity", RegexOptions.IgnoreCase).Should().Be(1, "the metadata row is the one place a page names a severity");
        });

    [Then("the schema version it gives is the version it ships under")]
    public void ThenTheSchemaVersionItGivesIsTheVersionItShipsUnder() =>
        _pages.Should().AllSatisfy(static page => page.Value("Schema version").Should().Be(page.VersionText()));

    private IReadOnlyList<SchemaVersion> _versions = [];
    private IReadOnlyList<RulePage> _pages = [];
}
