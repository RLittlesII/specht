using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Tests.Shared;

namespace Specht.Tests.Evaluation;

/// <summary>
/// The line a violation renders as (<c>0001-F2</c> B-001, B-016), through <see cref="SpechtViolation.ToString"/> over
/// violations built in memory: the identifier in square brackets after the message where the violation has one, and the
/// line ending at the message where it has none, on a line and on the file as a whole.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtViolationUnitTests
{
    /// <summary>Gets violations that carry an identifier, with the position and the severity word each renders.</summary>
    public static TheoryData<string, SpechtViolation, string, string> Identified =>
        new()
        {
            { "an error on a line", new SpechtViolationFixture().WithIdentifier("B-002").WithLine(7), "(7)", "error" },
            {
                "a warning on the file as a whole",
                new SpechtViolationFixture().WithIdentifier("0001-F2").WithSeverity(SpechtSeverity.Warning).WithLine(0),
                string.Empty,
                "warning"
            },
        };

    /// <summary>Gets violations that carry no identifier, with the position and the severity word each renders.</summary>
    public static TheoryData<string, SpechtViolation, string, string> Unidentified =>
        new()
        {
            { "an error on a line", new SpechtViolationFixture().WithIdentifier(null).WithLine(7), "(7)", "error" },
            {
                "a warning on the file as a whole",
                new SpechtViolationFixture().WithIdentifier(null).WithSeverity(SpechtSeverity.Warning).WithLine(0),
                string.Empty,
                "warning"
            },
        };

    [Theory]
    [MemberData(nameof(Identified))]
    public void AViolationWithAnIdentifier_WhenRendered_ShouldEndWithTheIdentifierInSquareBrackets(
        string because,
        SpechtViolation violation,
        string position,
        string severity)
    {
        // Given
        var expected = $"{violation.File}{position}: {severity} {violation.RuleId}: {violation.Message} [{violation.Identifier}]";

        // When
        var line = violation.ToString();

        // Then
        line.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(Unidentified))]
    public void AViolationWithNoIdentifier_WhenRendered_ShouldEndAtTheMessageWithNoBrackets(
        string because,
        SpechtViolation violation,
        string position,
        string severity)
    {
        // Given
        var expected = $"{violation.File}{position}: {severity} {violation.RuleId}: {violation.Message}";

        // When
        var line = violation.ToString();

        // Then
        line.Should().Be(expected, because);
    }
}
