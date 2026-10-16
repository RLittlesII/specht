using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// What a violation's <c>expected</c> carries for the frontmatter, section and table rules (<c>0001-F3</c> B-009, B-010,
/// B-011, B-026; decision 0004), over a model built in memory under a synthetic manifest and synthetic on-disk schemas,
/// evaluated by <see cref="SpechtRunner.Evaluate"/> and made a document by <see cref="SpecReportDocument.From"/>. The
/// manifest names its own schema files, sections, roles and headers, so a value the document carries is the manifest's and
/// not the default's. The schema sources are the integration tier's.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecReportDocumentExpectedUnitTests
{
    [Theory]
    [ClassData(typeof(FrontmatterFailures))]
    public void AFrontmatterTheSchemaRejects_WhenItsDocumentIsMade_ShouldExpectTheSchemaTheKeyAndOnlyTheAssertionsThatFailedAtThatPlace(
        string because,
        SpecModel model,
        string schema,
        string ruleId,
        string? key,
        JsonObject[] constraints)
    {
        // Given
        ISpecRule rule = new FrontmatterSchemaRule();

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [rule]));

        // Then
        var violations = document.Violations.Where(violation => violation.RuleId == ruleId && violation.Identifier == key).ToList();
        violations.Should().AllSatisfy(violation =>
        {
            (violation.Expected["schema"]?.GetValue<string>()).Should().Be(schema, because);
            violation.Expected.ContainsKey("key").Should().Be(key is not null, because);
            (violation.Expected["key"]?.GetValue<string>()).Should().Be(key, because);
        });
        violations.Select(static violation => Canonical(violation.Expected["constraints"])).Should()
            .BeEquivalentTo(constraints.Select(static expected => Canonical(expected)), because);
    }

    [Theory]
    [ClassData(typeof(MissingFrontmatters))]
    public void ADocumentWithNoFrontmatter_WhenItsDocumentIsMade_ShouldExpectItsKindsSchemaAndThatSchemasRequiredKeysInItsOrder(
        string because,
        SpecModel model,
        string schema,
        string[] required)
    {
        // Given
        ISpecRule rule = new FrontmatterSchemaRule();

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [rule]));

        // Then
        var expected = document.Violations.Should().ContainSingle(static violation => violation.RuleId == "SPEC001", because).Which.Expected;
        (expected["schema"]?.GetValue<string>()).Should().Be(schema, because);
        JsonNode.DeepEquals(expected["required"], new JsonArray([.. required.Select(static name => JsonValue.Create(name))])).Should().BeTrue(
            "{0}: required should be [{1}], but expected is {2}", because, string.Join(", ", required), expected.ToJsonString());
    }

    [Theory]
    [ClassData(typeof(SectionFailures))]
    public void ASectionViolation_WhenItsDocumentIsMade_ShouldExpectTheManifestsSectionTitlesInOrder(string because, FeatureSpec feature)
    {
        // Given
        SpecModel model = new SpecModelFixture()
            .WithManifest(new JsonObject
            {
                ["sections"] = new JsonArray("Goal", "Needs", "Design"),
                ["roles"] = new JsonObject { ["claims"] = "Goal", ["matrix"] = "Needs", ["signOff"] = "Design" },
            })
            .WithFeatures(feature);

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [new SectionStructureRule()]));

        // Then
        document.Violations.Where(static violation => violation.RuleId == "SPEC010").Should().NotBeEmpty(because)
            .And.AllSatisfy(violation => JsonNode.DeepEquals(violation.Expected["sections"], new JsonArray("Goal", "Needs", "Design")).Should().BeTrue(
                "{0}: sections should be the manifest's, in its order, but expected is {1}", because, violation.Expected.ToJsonString()));
    }

    [Theory]
    [ClassData(typeof(TableFailures))]
    public void ATableViolation_WhenItsDocumentIsMade_ShouldExpectTheSectionTitleAndTheHeadersTheManifestGivesItInOrder(
        string because,
        JsonObject manifest,
        FeatureSpec feature,
        string section,
        string[] headers)
    {
        // Given
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var document = SpecReportDocument.From(SpechtRunner.Evaluate(model, [new SectionStructureRule()]));

        // Then
        var expected = document.Violations.Should()
            .ContainSingle(violation => violation.RuleId == "SPEC013" && violation.Identifier == section, because).Which.Expected;
        (expected["section"]?.GetValue<string>()).Should().Be(section, because);
        JsonNode.DeepEquals(expected["headers"], new JsonArray([.. headers.Select(static header => JsonValue.Create(header))])).Should().BeTrue(
            "{0}: headers should be [{1}], but expected is {2}", because, string.Join(", ", headers), expected.ToJsonString());
    }

    private static string Canonical(JsonNode? node) =>
        node switch
        {
            JsonObject members => "{" + string.Join(",", members.OrderBy(static member => member.Key, StringComparer.Ordinal)
                .Select(static member => JsonSerializer.Serialize(member.Key) + ":" + Canonical(member.Value))) + "}",
            JsonArray elements => "[" + string.Join(",", elements.Select(Canonical)) + "]",
            null => "null",
            _ => node.ToJsonString(),
        };
}
