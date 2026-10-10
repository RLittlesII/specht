using AwesomeAssertions;
using Specht.Tests.Baseline;

namespace Specht.Tests.Runner;

/// <summary>
/// The engine's verdicts on the baseline tree against the golden report (<c>0001-F1</c> B-004, C-9, C-10), and the six
/// fields every violation carries (B-010).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtRunnerBaselineIntegrationTests
{
    /// <summary>Gets each rule, and the root-relative prefix of each layout the baseline tree breaks it in.</summary>
    public static TheoryData<string, string> Breaks
    {
        get
        {
            var data = new TheoryData<string, string>();

            foreach (var rule in Rules)
            {
                data.Add(rule, "epics/");

                if (rule is not ("SPEC004" or "SPEC011"))
                {
                    data.Add(rule, "src/");
                }
            }

            return data;
        }
    }

    [Fact]
    public void TheBaselineTree_WhenChecked_ShouldGiveTheGoldenReportsVerdictsFieldForFieldInOrder()
    {
        // Given
        using var tree = new SpecTree();
        BaselineTree.Write(tree);
        var golden = GoldenReport.Read();

        // When
        var verdicts = GoldenReport.Of(tree.Run());

        // Then
        verdicts.Should().Equal(golden);
    }

    [Theory]
    [MemberData(nameof(Breaks))]
    public void TheBaselineTree_WhenChecked_ShouldBreakTheRuleInTheLayout(string rule, string layout)
    {
        // Given
        using var tree = new SpecTree();
        BaselineTree.Write(tree);

        // When
        var report = tree.Run();

        // Then
        report.Violations.Should().Contain(violation => violation.RuleId == rule && violation.File.StartsWith(layout, StringComparison.Ordinal));
    }

    [Fact]
    public void AClaimWithNoTraceabilityRow_WhenChecked_ShouldCarryARuleSeverityFileLineIdentifierAndMessage()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteFeature(
            "0001",
            "F1",
            sections: SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another. | brd | Active |\n"));

        // When
        var violations = tree.Run().Violations;

        // Then
        var violation = violations.Should().ContainSingle().Subject;
        violation.RuleId.Should().NotBeNullOrWhiteSpace();
        violation.Severity.Should().BeDefined();
        violation.File.Should().NotBeNullOrWhiteSpace();
        violation.Line.Should().BePositive();
        violation.Identifier.Should().NotBeNullOrWhiteSpace();
        violation.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void EveryViolationOnTheBaselineTree_WhenChecked_ShouldCarryARuleSeverityFileLineAndMessage()
    {
        // Given
        using var tree = new SpecTree();
        BaselineTree.Write(tree);

        // When
        var violations = tree.Run().Violations;

        // Then
        violations.Should().NotBeEmpty().And.AllSatisfy(static violation =>
        {
            violation.RuleId.Should().MatchRegex("^SPEC[0-9]{3}$");
            violation.Severity.Should().BeDefined();
            violation.File.Should().NotBeNullOrWhiteSpace().And.NotStartWith("/").And.NotContain("\\");
            Path.IsPathRooted(violation.File).Should().BeFalse(violation.File);
            violation.Line.Should().BeGreaterThanOrEqualTo(0);
            violation.Message.Should().NotBeNullOrWhiteSpace();
            (violation.Identifier is null || violation.Identifier.Trim().Length > 0).Should().BeTrue(violation.ToString());
        });
    }

    private static readonly string[] Rules =
    [
        "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC030",
        "SPEC031", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051", "SPEC052", "SPEC060", "SPEC061",
    ];
}
