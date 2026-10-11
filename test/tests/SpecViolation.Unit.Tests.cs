using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests;

/// <summary>
/// The line a violation renders as (<c>0001-F2</c> B-001, B-016), through <see cref="SpecViolation.ToString"/> over
/// violations built in memory: the identifier in square brackets after the message where the violation has one, and the
/// line ending at the message where it has none, on a line and on the file as a whole.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecViolationUnitTests
{
    /// <summary>Gets violations that carry an identifier, with the position and the severity word each renders.</summary>
    public static TheoryData<string, SpecViolation, string, string> Identified =>
        new()
        {
            { "an error on a line", new SpecViolationFixture().WithIdentifier("B-002").WithLine(7), "(7)", "error" },
            {
                "a warning on the file as a whole",
                new SpecViolationFixture().WithIdentifier("0001-F2").WithSeverity(SpecSeverity.Warning).WithLine(0),
                string.Empty,
                "warning"
            },
        };

    /// <summary>Gets violations that carry no identifier, with the position and the severity word each renders.</summary>
    public static TheoryData<string, SpecViolation, string, string> Unidentified =>
        new()
        {
            { "an error on a line", new SpecViolationFixture().WithIdentifier(null).WithLine(7), "(7)", "error" },
            {
                "a warning on the file as a whole",
                new SpecViolationFixture().WithIdentifier(null).WithSeverity(SpecSeverity.Warning).WithLine(0),
                string.Empty,
                "warning"
            },
        };

    [Theory]
    [MemberData(nameof(Identified))]
    public void AViolationWithAnIdentifier_WhenRendered_ShouldEndWithTheIdentifierInSquareBrackets(
        string because,
        SpecViolation violation,
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
        SpecViolation violation,
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
