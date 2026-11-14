using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// The serialized report document over a check on a tree on disk, and against the published schema (<c>0001-F3</c> B-005,
/// B-006, B-008, B-021; item <c>0167</c>). The schema is <c>docs/schema/report.schema.json</c> (A-1), copied into the
/// output from that one file.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtReportSerializerIntegrationTests
{
    /// <summary>Gets reports of each shape a document is made from: empty, and with violations of both severities.</summary>
    public static TheoryData<string, SpechtReport> Reports =>
        new()
        {
            { "a run that found nothing", new SpechtReportFixture() },
            {
                "a run with an error carrying an identifier and a warning on a whole file",
                new SpechtReportFixture()
                    .WithSpecificationCount(2)
                    .WithLayouts(new SpechtReportLayout("epics", 1), new SpechtReportLayout("features", 1))
                    .WithItemCount(1)
                    .WithRulesEvaluated(21)
                    .WithViolations(
                        new SpechtViolationFixture().WithIdentifier("B-002").WithLine(12),
                        new SpechtViolationFixture().WithSeverity(SpechtSeverity.Warning).WithFile("src/area/.spec/README.md").WithLine(0))
            },
            {
                "a run over the local copy of an upstream schema source (0001-F7 B-032)",
                (SpechtReport)new SpechtReportFixture() with { SchemaSource = SpechtSchemaSource.Upstream }
            },
        };

    [Fact]
    public void TheSameTreeUnderTwoRoots_WhenEachDocumentIsSerialized_ShouldBeIdentical()
    {
        // Given
        using var first = new SpechtTree();
        using var second = new SpechtTree();
        first.WriteFeature("0001", "F1", featureFile: null);
        second.WriteFeature("0001", "F1", featureFile: null);

        // When
        var documents = new[] { first, second }
            .Select(static tree => new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(tree.Run()))).ToList();

        // Then
        documents[1].Should().Be(documents[0]);
    }

    [Theory]
    [MemberData(nameof(Reports))]
    public void AReport_WhenItsDocumentIsSerialized_ShouldValidateAgainstThePublishedReportSchema(string because, SpechtReport report)
    {
        // Given
        var path = Path.Combine(AppContext.BaseDirectory, "docs", "schema", "report.schema.json");
        File.Exists(path).Should().BeTrue("the report schema is published at docs/schema/report.schema.json (A-1)");
        var schema = JsonSchema.FromText(File.ReadAllText(path));

        // When
        using var json = JsonDocument.Parse(new SpechtReportSerializer().Serialize(new SpechtReportDocumentMapper().Map(report)));
        var result = schema.Evaluate(json.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });

        // Then
        result.IsValid.Should().BeTrue(
            "{0}: {1}",
            because,
            string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
                .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}"))));
    }

    [Fact]
    public void ATreeWithViolationsInBothLayouts_WhenItsDocumentIsSerialized_ShouldCarryOnlyRootRelativeForwardSlashPaths()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteFeature("0001", "F1", featureFile: null);
        tree.WriteCoLocatedFeature("src/a/deeply/nested/area", "0002", "F1", new Dictionary<string, string> { ["spec_status"] = "nearly" });

        // When
        var document = new SpechtReportDocumentMapper().Map(tree.Run());
        var json = new SpechtReportSerializer().Serialize(document);

        // Then
        document.Violations.Select(static violation => violation.File).Should()
            .Contain(["epics/0001-epic/F1-feature/spec.md", "src/a/deeply/nested/area/.spec/README.md"])
            .And.AllSatisfy(static file =>
            {
                Path.IsPathRooted(file).Should().BeFalse(file);
                file.Should().NotContain("\\");
            });
        Strings(JsonNode.Parse(json)).Should().NotContain(value => value.Contains(tree.Root, StringComparison.Ordinal));
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
