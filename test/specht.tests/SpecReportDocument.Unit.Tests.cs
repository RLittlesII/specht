using System.Globalization;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using specht.Report;

namespace specht.tests;

/// <summary>
/// The report document made from a report built in memory (<c>0001-F3</c> B-005, B-006, B-007): what it counts, what a
/// violation carries, and that nothing in it comes from the clock or the machine.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecReportDocumentUnitTests
{
    /// <summary>Gets violations of each shape a rule reports: with and without an identifier and a line.</summary>
    public static TheoryData<string, SpecViolation> Violations =>
        new()
        {
            { "an error with an identifier and a line", new SpecViolationFixture().WithIdentifier("B-002").WithLine(7) },
            {
                "a warning on the file as a whole, with no identifier",
                new SpecViolationFixture().WithSeverity(SpecSeverity.Warning).WithFile("src/area/.spec/README.md").WithLine(0)
            },
        };

    [Fact]
    public void AReport_WhenMadeADocument_ShouldCarryEachLayoutsCountTheItemsTheRulesEvaluatedAndTheSeverityCounts()
    {
        // Given
        SpecCheckReport report = new SpecCheckReportFixture()
            .WithSpecificationCount(3)
            .WithLegacyCount(2)
            .WithCoLocatedCount(1)
            .WithItemCount(4)
            .WithRulesEvaluated(9)
            .WithViolations(
                new SpecViolationFixture(),
                new SpecViolationFixture().WithLine(2),
                new SpecViolationFixture().WithSeverity(SpecSeverity.Warning));

        // When
        var document = SpecReportDocument.From(report);

        // Then
        document.Layouts.Should().BeEquivalentTo([new SpecReportLayout(SpecLayout.Legacy, 2), new SpecReportLayout(SpecLayout.CoLocated, 1)]);
        document.ItemCount.Should().Be(4);
        document.RulesEvaluated.Should().Be(9);
        document.ErrorCount.Should().Be(2);
        document.WarningCount.Should().Be(1);
        document.Violations.Should().HaveCount(3);
    }

    [Theory]
    [MemberData(nameof(Violations))]
    public void AViolation_WhenMadeADocument_ShouldCarryItsRuleSeverityFileLineIdentifierMessageAndAnExpectedObject(
        string because,
        SpecViolation violation)
    {
        // Given
        SpecCheckReport report = new SpecCheckReportFixture().WithViolations(violation);

        // When
        var document = SpecReportDocument.From(report);

        // Then
        var carried = document.Violations.Should().ContainSingle(because).Which;
        carried.RuleId.Should().Be(violation.RuleId, because);
        carried.Severity.Should().Be(violation.Severity, because);
        carried.File.Should().Be(violation.File, because);
        carried.Line.Should().Be(violation.Line, because);
        carried.Identifier.Should().Be(violation.Identifier, because);
        carried.Message.Should().Be(violation.Message, because);
        carried.Expected.Should().NotBeNull(because);
    }

    [Fact]
    public void AReport_WhenItsDocumentIsSerialized_ShouldCarryNoValueFromTheClockOrTheMachine()
    {
        // Given
        SpecCheckReport report = new SpecCheckReportFixture()
            .WithSpecificationCount(1)
            .WithLegacyCount(1)
            .WithRulesEvaluated(1)
            .WithViolations(new SpecViolationFixture().WithIdentifier("B-002"));
        string[] machine =
        [
            Environment.MachineName,
            Environment.UserName,
            Environment.CurrentDirectory,
            Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ];

        // When
        var values = Strings(JsonNode.Parse(SpecReportDocument.From(report).ToJson())).ToList();

        // Then
        values.Where(static value => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            .Should().BeEmpty("no value in the document is read from the clock");
        values.Where(value => machine.Any(source => source.Length > 0 && value.Contains(source, StringComparison.OrdinalIgnoreCase)))
            .Should().BeEmpty("no value in the document is read from the machine or the environment");
    }

    private static IEnumerable<string> Strings(JsonNode? node) =>
        node switch
        {
            JsonObject members => members.SelectMany(static member => Strings(member.Value)),
            JsonArray elements => elements.SelectMany(Strings),
            JsonValue value when value.TryGetValue<string>(out var text) => [text],
            _ => [],
        };
}
