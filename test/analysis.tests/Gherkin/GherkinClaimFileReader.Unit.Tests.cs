using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Bindings.Gherkin;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class GherkinClaimFileReaderUnitTests
{
    [Fact]
    public void ScenarioTaggedWithAClaim_WhenRead_ShouldLocateTheClaimAtTheScenarioKeywordLine()
    {
        // Given
        var text = """
            Feature: Arithmetic

              @REQ-FIX-002
              Scenario: Subtract
                Given a number
            """;

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Id.Should().Be("REQ-FIX-002");
        claim.Element.Should().Be(BindingClaimElement.Scenario);
        claim.RelativePath.Should().Be(Path);
        claim.Line.Should().Be(4);
        claim.State.Should().Be("Active");
        claim.Cases.Should().Equal(new BindingClaimCase(4, false));
    }

    [Fact]
    public void OutlineWithTwoExampleRows_WhenRead_ShouldDeclareOneClaimWithOneCasePerRow()
    {
        // Given
        var text = """
            Feature: Arithmetic

              @REQ-FIX-001
              Scenario Outline: Add
                Given <a>

                Examples:
                  | a |
                  | 1 |
                  | 2 |
            """;

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Element.Should().Be(BindingClaimElement.ScenarioOutline);
        claim.Line.Should().Be(4);
        claim.Cases.Should().Equal(new BindingClaimCase(9, false), new BindingClaimCase(10, false));
    }

    [Fact]
    public void OutlineWithHeaderOnlyExamples_WhenRead_ShouldHaveNoCases()
    {
        // Given
        var text = """
            Feature: Arithmetic

              @REQ-FIX-001
              Scenario Outline: Add
                Given <a>

                Examples:
                  | a |
            """;

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Cases.Should().BeEmpty();
    }

    [Fact]
    public void RuleTaggedWithAClaim_WhenRead_ShouldDeclareOneClaimAtTheRuleHoldingEveryScenarioCase()
    {
        // Given
        var text = """
            Feature: Arithmetic

              @REQ-FIX-003
              Rule: Addition returns the sum

                Scenario: One
                  Given a

                Scenario: Two
                  Given b
            """;

        // When
        var file = Reader().Read(Path, text);

        // Then
        var claim = file.Claims.Should().ContainSingle().Subject;
        claim.Element.Should().Be(BindingClaimElement.Rule);
        claim.Line.Should().Be(4);
        claim.Cases.Should().Equal(new BindingClaimCase(6, false), new BindingClaimCase(9, false));
        file.Findings.Should().BeEmpty();
    }

    [Fact]
    public void FeatureTaggedWithAClaim_WhenRead_ShouldDeclareItAtTheFeatureAndCoverItsScenarios()
    {
        // Given
        var text = "@spec.feature:arithmetic @spec.epic:0201 @REQ-FIX-009\nFeature: Arithmetic\n  Scenario: S\n    Given x\n"
            + "\n  @spec.feature:other\n  Scenario: T\n    Given y\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        var claim = file.Claims.Should().ContainSingle().Subject;
        claim.Id.Should().Be("REQ-FIX-009");
        claim.Element.Should().Be(BindingClaimElement.Feature);
        claim.Line.Should().Be(2);
        claim.Cases.Should().Equal(new BindingClaimCase(3, false), new BindingClaimCase(7, false));
        file.Findings.Should().BeEmpty();
        file.FeatureTagValues.Should().Equal("arithmetic");
        file.EpicTagValues.Should().Equal("0201");
    }

    [Fact]
    public void PolicyWithoutScopeForms_WhenFeatureTagsAreRead_ShouldCaptureNoScopeValues()
    {
        // Given
        GherkinClaimPolicy policy = new GherkinClaimPolicyFixture().WithFeatureScope(null).WithEpicScope(null);

        // When
        var file = new GherkinClaimFileReader(policy).Read(Path, "@spec.feature:arithmetic\nFeature: A\n");

        // Then
        file.FeatureTagValues.Should().BeEmpty();
        file.EpicTagValues.Should().BeEmpty();
    }

    [Fact]
    public void OneClaimTaggedTwiceOnOneElement_WhenRead_ShouldDeclareItOnce()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001 @REQ-FIX-001\n  Scenario: S\n    Given x\n";

        // When
        var claims = Reader().Read(Path, text).Claims;

        // Then
        claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-001");
    }

    [Fact]
    public void ElementsCarryingSeveralClaims_WhenRead_ShouldListClaimsInDocumentOrderThenTagOrder()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-002 @REQ-FIX-001\n  Scenario: S\n    Given x\n\n  @REQ-FIX-003\n  Scenario: T\n    Given y\n";

        // When
        var claims = Reader().Read(Path, text).Claims;

        // Then
        claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-002", "REQ-FIX-001", "REQ-FIX-003");
        claims.Select(static claim => claim.Line).Should().Equal(4, 4, 8);
    }

    [Fact]
    public void RetitledAndReorderedScenarios_WhenRead_ShouldKeepEveryClaimIdentity()
    {
        // Given
        var before = "Feature: A\n\n  @REQ-FIX-001\n  Scenario: First\n    Given x\n\n  @REQ-FIX-002\n  Scenario: Second\n    Given y\n";
        var after = "Feature: A\n\n  @REQ-FIX-002\n  Scenario: Renamed second\n    Given y\n\n  @REQ-FIX-001\n  Scenario: Renamed first\n    Given x\n";

        // When
        var original = Reader().Read(Path, before).Claims;
        var changed = Reader().Read(Path, after).Claims;

        // Then
        changed.Select(static claim => claim.Id).Should().BeEquivalentTo(original.Select(static claim => claim.Id));
        changed.Single(static claim => claim.Id == "REQ-FIX-001").Line.Should().Be(8);
        original.Single(static claim => claim.Id == "REQ-FIX-001").Line.Should().Be(4);
    }

    [Fact]
    public void ClaimFileWithWindowsLineEndings_WhenRead_ShouldGiveTheSameClaimsAndLines()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001\n  Scenario Outline: S\n    Given <a>\n\n    Examples:\n      | a |\n      | 1 |\n";

        // When
        var unix = Reader().Read(Path, text);
        var windows = Reader().Read(Path, text.Replace("\n", "\r\n"));

        // Then
        windows.Claims.Should().BeEquivalentTo(unix.Claims);
    }

    [Fact]
    public void MixedOrdinaryAndClaimTags_WhenRead_ShouldDeclareOnlyTheClaimAndDrawNoFinding()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-004 @visual @ignore @boundary @issue-42\n  Scenario: S\n    Given x\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-004");
        file.Findings.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ConfiguredClaimIdCases))]
    public void ConfiguredClaimForm_WhenAScenarioTagIsRead_ShouldDeclareOnlyIdsTheFormAndGrammarAccept(
        string form, string grammar, string tag, string? expected)
    {
        // Given
        GherkinClaimPolicy policy = new GherkinClaimPolicyFixture().WithClaim(new GherkinTagForm(form, "claim", false, grammar));
        var text = "Feature: A\n\n  " + tag + "\n  Scenario: S\n    Given x\n";

        // When
        var file = new GherkinClaimFileReader(policy).Read(Path, text);

        // Then
        file.Claims.Select(static claim => claim.Id).Should().Equal(expected is null ? [] : [expected]);
    }

    [Fact]
    public void TagFailingTheClaimGrammar_WhenRead_ShouldDeclareNoClaimAndNameNoTagInAFinding()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-5\n  Scenario: S\n    Given x\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Claims.Should().BeEmpty();
        file.Findings.Should().NotContain(static finding => finding.DiagnosticId == "SPCT0011");
        file.Findings.Should().NotContain(static finding => finding.Text.Contains("@REQ-FIX-5", StringComparison.Ordinal));
    }

    [Fact]
    public void ClaimTagOnAnExamplesBlock_WhenRead_ShouldReportItAtTheTagLineAndDeclareNothing()
    {
        // Given
        var text = """
            Feature: A

              @REQ-FIX-001
              Scenario Outline: Add
                Given <a>

                @REQ-FIX-006
                Examples:
                  | a |
                  | 1 |
            """;

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-001");
        file.Findings.Should().Equal(new BindingFinding("SPCT0011", Path, 7,
            "Tag @REQ-FIX-006 declares a claim where no claim can be declared."));
    }

    [Fact]
    public void ScenarioWithoutAnyClaim_WhenRead_ShouldReportAMissingClaimAtItsKeywordLine()
    {
        // Given
        var text = "Feature: A\n\n  Scenario: Unclaimed\n    Given x\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Claims.Should().BeEmpty();
        file.Findings.Should().Equal(new BindingFinding("SPCT0012", Path, 3, "Scenario \"Unclaimed\" declares no claim."));
    }

    [Fact]
    public void OutlineWithoutAnyClaim_WhenRead_ShouldReportAMissingClaimNamingTheOutline()
    {
        // Given
        var text = "Feature: A\n\n  Scenario Outline: Unclaimed\n    Given <a>\n\n    Examples:\n      | a |\n      | 1 |\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Findings.Should().Equal(new BindingFinding("SPCT0012", Path, 3, "Scenario Outline \"Unclaimed\" declares no claim."));
    }

    [Theory]
    [InlineData("Feature: A\n\n  @REQ-FIX-003\n  Rule: R\n\n    Scenario: S\n      Given x\n")]
    [InlineData("@REQ-FIX-004\nFeature: A\n\n  Scenario: S\n    Given x\n")]
    public void ScenarioCoveredByItsRuleOrFeatureClaim_WhenRead_ShouldReportNoMissingClaim(string text)
    {
        // Given

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Claims.Should().ContainSingle();
        file.Findings.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Active")]
    [InlineData("@withdrawn", "Withdrawn")]
    [InlineData("@superseded", "Superseded")]
    [InlineData("@withdrawn @superseded", "Superseded")]
    [InlineData("@visual", "Active")]
    public void ClaimWithConfiguredLifecycleTags_WhenRead_ShouldGiveTheStrongestState(string tags, string state)
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001 " + tags + "\n  Scenario: S\n    Given x\n";

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.State.Should().Be(state);
    }

    [Fact]
    public void LifecycleTagOnARule_WhenAScenarioInsideDeclaresItsOwnClaim_ShouldNotChangeTheScenarioClaimState()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-003 @withdrawn\n  Rule: R\n\n    @REQ-FIX-002\n    Scenario: S\n      Given x\n";

        // When
        var claims = Reader().Read(Path, text).Claims;

        // Then
        claims.Single(static claim => claim.Id == "REQ-FIX-003").State.Should().Be("Withdrawn");
        claims.Single(static claim => claim.Id == "REQ-FIX-002").State.Should().Be("Active");
    }

    [Fact]
    public void ScenarioTaggedIgnored_WhenRead_ShouldStillDeclareItsClaimWithPlannedCases()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001 @ignore\n  Scenario Outline: S\n    Given <a>\n\n    Examples:\n      | a |\n      | 1 |\n      | 2 |\n";

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Id.Should().Be("REQ-FIX-001");
        claim.State.Should().Be("Active");
        claim.Cases.Should().Equal(new BindingClaimCase(9, true), new BindingClaimCase(10, true));
    }

    [Fact]
    public void OutlineWithOneIgnoredExamplesBlock_WhenRead_ShouldPlanOnlyThatBlocksRows()
    {
        // Given
        var text = """
            Feature: A

              @REQ-FIX-001
              Scenario Outline: Add
                Given <a>

                Examples: required
                  | a |
                  | 1 |

                @ignore
                Examples: later
                  | a |
                  | 2 |
            """;

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Findings.Should().BeEmpty();
        var claim = file.Claims.Should().ContainSingle().Subject;
        claim.State.Should().Be("Active");
        claim.Cases.Should().Equal(new BindingClaimCase(9, false), new BindingClaimCase(14, true));
    }

    [Theory]
    [InlineData("@ignore\nFeature: A\n\n  @REQ-FIX-001\n  Scenario: S\n    Given x\n", 5)]
    [InlineData("Feature: A\n\n  @ignore\n  Rule: R\n\n    @REQ-FIX-001\n    Scenario: S\n      Given x\n", 7)]
    public void CaseUnderAnIgnoredFeatureOrRule_WhenRead_ShouldBePlanned(string text, int line)
    {
        // Given

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Cases.Should().Equal(new BindingClaimCase(line, true));
    }

    [Fact]
    public void EmptyPlannedTagList_WhenAnIgnoredTagIsRead_ShouldPlanNothing()
    {
        // Given
        GherkinClaimPolicy policy = new GherkinClaimPolicyFixture().WithPlannedTags([]);
        var text = "Feature: A\n\n  @REQ-FIX-001 @ignore\n  Scenario: S\n    Given x\n";

        // When
        var claim = new GherkinClaimFileReader(policy).Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Cases.Should().Equal(new BindingClaimCase(4, false));
    }

    [Theory]
    [InlineData("Scenario Template", "Examples", BindingClaimElement.ScenarioOutline)]
    [InlineData("Scenario", "Examples", BindingClaimElement.ScenarioOutline)]
    [InlineData("Example", "Scenarios", BindingClaimElement.ScenarioOutline)]
    [InlineData("Scenario Outline", "Examples", BindingClaimElement.ScenarioOutline)]
    public void ScenarioKeywordsWithAnExamplesBlock_WhenRead_ShouldDeclareAnOutline(string keyword, string examples, BindingClaimElement element)
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001\n  " + keyword + ": S\n    Given <a>\n\n    " + examples + ":\n      | a |\n      | 1 |\n";

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Element.Should().Be(element);
        claim.Cases.Should().Equal(new BindingClaimCase(9, false));
    }

    [Fact]
    public void ExampleKeywordWithoutExamples_WhenRead_ShouldDeclareAScenario()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001\n  Example: S\n    Given x\n";

        // When
        var claim = Reader().Read(Path, text).Claims.Should().ContainSingle().Subject;

        // Then
        claim.Element.Should().Be(BindingClaimElement.Scenario);
    }

    [Fact]
    public void TablesOutsideExamples_WhenRead_ShouldAddNoCases()
    {
        // Given
        var text = "Feature: A\n\n  Background:\n    Given a table\n      | x |\n      | 1 |\n\n"
            + "  @REQ-FIX-001\n  Scenario: S\n    Given a table\n      | x |\n      | 1 |\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Findings.Should().BeEmpty();
        file.Claims.Should().ContainSingle().Which.Cases.Should().Equal(new BindingClaimCase(9, false));
    }

    [Fact]
    public void TagsAndKeywordsInsideADocString_WhenRead_ShouldBeContentOnly()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001\n  Scenario: S\n    Given text\n      \"\"\"\n      @REQ-FIX-009\n      Scenario: Not real\n      \"\"\"\n";

        // When
        var file = Reader().Read(Path, text);

        // Then
        file.Claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-001");
        file.Findings.Should().BeEmpty();
    }

    [Fact]
    public void CommentOnATagLine_WhenRead_ShouldEndTheTagsAtTheComment()
    {
        // Given
        var text = "Feature: A\n\n  @REQ-FIX-001 # @REQ-FIX-002\n  Scenario: S\n    Given x\n";

        // When
        var claims = Reader().Read(Path, text).Claims;

        // Then
        claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-001");
    }

    [Theory]
    [InlineData("# language: fr\nFeature: A\n")]
    [InlineData("#language:fr\nFeature: A\n")]
    [InlineData("# language : fr\nFeature: A\n")]
    [InlineData("# language: fr\nthis is not gherkin\n")]
    public void ClaimFileDeclaringAnotherLanguage_WhenRead_ShouldFailNamingTheFileAndTheLanguage(string text)
    {
        // Given

        // When
        Action read = () => Reader().Read(Path, text);

        // Then
        var failure = read.Should().Throw<GherkinInputException>().Which;
        failure.RelativePath.Should().Be(Path);
        failure.Message.Should().Be("Claim file declares a Gherkin language other than English.");
    }

    [Theory]
    [InlineData("# language: en\nFeature: A\n")]
    [InlineData("Feature: A\n# language: fr\n")]
    [InlineData("Feature: A\n")]
    public void ClaimFileInEnglishOrWithoutALanguageHeader_WhenRead_ShouldNotFail(string text)
    {
        // Given

        // When
        Action read = () => Reader().Read(Path, text);

        // Then
        read.Should().NotThrow();
    }

    [Theory]
    [ClassData(typeof(InvalidClaimFiles))]
    public void ClaimFileBreakingAListedCause_WhenRead_ShouldFailNamingTheFileAsNotValidGherkin(string text)
    {
        // Given

        // When
        Action read = () => Reader().Read(Path, text);

        // Then
        var failure = read.Should().Throw<GherkinInputException>().Which;
        failure.RelativePath.Should().Be(Path);
        failure.Message.Should().Be("Claim file is not valid Gherkin.");
    }

    private static GherkinClaimFileReader Reader()
    {
        GherkinClaimPolicy policy = new GherkinClaimPolicyFixture();
        return new GherkinClaimFileReader(policy);
    }

    private const string Path = "specifications/arithmetic.feature";
}
