using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Evaluation;

/// <summary>
/// The order the engine reports violations in (<c>0001-F1</c> B-004, C-9), through <see cref="SpechtViolationOrder.Order"/>
/// over violations built in memory: severity descending, then file, line and rule id, file and rule id compared
/// ordinally, and violations equal on all four keys left in the order they were given.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtViolationOrderUnitTests
{
    /// <summary>Gets two violations that differ first on one key, the one that sorts first given first.</summary>
    public static TheoryData<string, SpechtViolation, SpechtViolation> Pairs =>
        new()
        {
            {
                "an error comes before a warning, whatever its file, line and rule id",
                new SpechtViolationFixture().WithFile("z/spec.md").WithLine(9).WithRuleId("SPEC061"),
                new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning).WithFile("a/spec.md").WithLine(1).WithRuleId("SPEC001")
            },
            {
                "files compare ordinally, so an upper-case path comes before a lower-case one, whatever the line and rule id",
                new SpechtViolationFixture().WithFile("B/spec.md").WithLine(9).WithRuleId("SPEC061"),
                new SpechtViolationFixture().WithFile("a/spec.md").WithLine(1).WithRuleId("SPEC001")
            },
            {
                "lines ascend within a file, whatever the rule id",
                new SpechtViolationFixture().WithLine(2).WithRuleId("SPEC061"),
                new SpechtViolationFixture().WithLine(10).WithRuleId("SPEC001")
            },
            {
                "rule ids compare ordinally within a line, so an upper-case id comes before a lower-case one",
                new SpechtViolationFixture().WithRuleId("SPEC031"),
                new SpechtViolationFixture().WithRuleId("spec010")
            },
        };

    [Theory]
    [MemberData(nameof(Pairs))]
    public void TwoViolationsGivenOutOfOrder_WhenOrdered_ShouldPutTheOneThatSortsFirstFirst(
        string because,
        SpechtViolation first,
        SpechtViolation second)
    {
        // Given
        SpechtViolation[] violations = [second, first];

        // When
        var ordered = new SpechtViolationOrder().Order(violations);

        // Then
        ordered.Should().Equal([first, second], because);
    }

    [Fact]
    public void ViolationsEqualOnSeverityFileLineAndRuleId_WhenOrdered_ShouldKeepTheOrderTheyWereGivenIn()
    {
        // Given
        SpechtViolation warning = new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning).WithMessage("a warning");
        SpechtViolation[] tied =
        [
            .. Enumerable.Range(1, 20).Select(static index => (SpechtViolation)new SpechtViolationFixture().WithMessage($"collected {index}")),
        ];
        SpechtViolation[] violations = [.. tied[..10], warning, .. tied[10..]];

        // When
        var ordered = new SpechtViolationOrder().Order(violations);

        // Then
        ordered.Should().Equal([.. tied, warning]);
    }
}
