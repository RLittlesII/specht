using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The order the engine reports violations in (<c>0001-F1</c> B-004, C-9), through <see cref="SpechtRunner.Order"/>
/// over violations built in memory: severity descending, then file, line and rule id, file and rule id compared
/// ordinally, and violations equal on all four keys left in the order they were given.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerOrderUnitTests
{
    /// <summary>Gets two violations that differ first on one key, the one that sorts first given first.</summary>
    public static TheoryData<string, SpecViolation, SpecViolation> Pairs =>
        new()
        {
            {
                "an error comes before a warning, whatever its file, line and rule id",
                new SpecViolationFixture().WithFile("z/spec.md").WithLine(9).WithRuleId("SPEC061"),
                new SpecViolationFixture().WithSeverity(SpecSeverity.Warning).WithFile("a/spec.md").WithLine(1).WithRuleId("SPEC001")
            },
            {
                "files compare ordinally, so an upper-case path comes before a lower-case one, whatever the line and rule id",
                new SpecViolationFixture().WithFile("B/spec.md").WithLine(9).WithRuleId("SPEC061"),
                new SpecViolationFixture().WithFile("a/spec.md").WithLine(1).WithRuleId("SPEC001")
            },
            {
                "lines ascend within a file, whatever the rule id",
                new SpecViolationFixture().WithLine(2).WithRuleId("SPEC061"),
                new SpecViolationFixture().WithLine(10).WithRuleId("SPEC001")
            },
            {
                "rule ids compare ordinally within a line, so an upper-case id comes before a lower-case one",
                new SpecViolationFixture().WithRuleId("SPEC031"),
                new SpecViolationFixture().WithRuleId("spec010")
            },
        };

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TwoViolationsGivenOutOfOrder_WhenOrdered_ShouldPutTheOneThatSortsFirstFirst(
        string because,
        SpecViolation first,
        SpecViolation second)
    {
        // Given
        SpecViolation[] violations = [second, first];

        // When
        var ordered = SpechtRunner.Order(violations);

        // Then
        ordered.Should().Equal([first, second], because);
    }

    [Fact]
    public void ViolationsEqualOnSeverityFileLineAndRuleId_WhenOrdered_ShouldKeepTheOrderTheyWereGivenIn()
    {
        // Given
        SpecViolation warning = new SpecViolationFixture().WithSeverity(SpecSeverity.Warning).WithMessage("a warning");
        SpecViolation[] tied =
        [
            .. Enumerable.Range(1, 20).Select(static index => (SpecViolation)new SpecViolationFixture().WithMessage($"collected {index}")),
        ];
        SpecViolation[] violations = [.. tied[..10], warning, .. tied[10..]];

        // When
        var ordered = SpechtRunner.Order(violations);

        // Then
        ordered.Should().Equal([.. tied, warning]);
    }
}
