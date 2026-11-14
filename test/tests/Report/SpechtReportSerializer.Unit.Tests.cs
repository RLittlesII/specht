using System.Globalization;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// The report document serialized from a report built in memory (<c>0001-F3</c> B-006; item <c>0167</c>): nothing in it
/// comes from the clock or the machine.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtReportSerializerUnitTests
{
    [Fact]
    public void AReport_WhenItsDocumentIsSerialized_ShouldCarryNoValueFromTheClockOrTheMachine()
    {
        // Given
        SpechtReport report = new SpechtReportFixture()
            .WithSpecificationCount(1)
            .WithLayouts(new SpechtReportLayout("epics", 1), new SpechtReportLayout("features", 0))
            .WithRulesEvaluated(1)
            .WithViolations(new SpechtViolationFixture().WithIdentifier("B-002"));
        string[] machine =
        [
            Environment.MachineName,
            Environment.UserName,
            Environment.CurrentDirectory,
            Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar),
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ];

        // When
        var values = Strings(JsonNode.Parse(new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(report)))).ToList();

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
