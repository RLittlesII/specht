using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Specht.Composition;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Tests.Rules.Matrix;

/// <summary>Waiver and retirement decisions over parsed snapshots with consumer-owned names (0001-F12 B-018 to B-020).</summary>
[Trait("Tier", "Unit")]
public sealed class WaiverAndRetiredRulesUnitTests
{
    [Theory]
    [ClassData(typeof(WaiverCases))]
    public void AWaiver_WhenEvaluated_ShouldRequireADatedSignedOffApproverUsingConfiguredColumnsAndMarkers(
        string status, string cell, string signOff, string approver, bool finding)
    {
        // Given
        var manifest = JsonNode.Parse("""
            {"schemaVersion":"0.2.0","sections":["3. Acceptance Criteria","9. Coverage","12. Decisions"],
             "roles":{"matrix":"9. Coverage","signOff":"12. Decisions"},
             "tables":{"matrix":["ID","Case","Evidence","State"],"signOff":["Area","Verdict","Approver","Reason"]},
             "markers":{"waived":"Excused","draft":"DraftState","blocked":"BlockedState"}}
            """)!.AsObject();
        var text = $$"""
            ## 9. Coverage
            | ID | Case | Evidence | State |
            | --- | --- | --- | --- |
            | B-018 | A waiver | {{cell}} | {{status}} |
            ## 12. Decisions
            | Area | Verdict | Approver | Reason |
            | --- | --- | --- | --- |
            | 1-5 | {{signOff}} | {{approver}} | Decision |
            """;
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpechtDocument.Parse(text, "src/sample/.spec/README.md"));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        using var services = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();
        var rule = ((IEnumerable<ISpechtRule>)services.Resolve(typeof(IEnumerable<ISpechtRule>))!).SingleOrDefault(static rule => rule.Id == "SPEC034");

        // When
        rule.Should().NotBeNull("schema 0.2.0 ships the waiver rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        findings.Should().HaveCount(finding ? 1 : 0);
        rule.DefaultLevel.Should().Be(SpechtRuleLevel.Off);
        if (finding)
        {
            findings[0].Identifier.Should().Be("B-018");
            findings[0].Line.Should().Be(4);
            findings[0].File.Should().Be(feature.RelativePath);
        }
    }

    [Theory]
    [ClassData(typeof(RetiredClaimCases))]
    public void ARetiredClaim_WhenEvaluated_ShouldReportARemainingTagOrDifferentMatrixMarkerOnce(
        string status, string matrix, string? tag, bool ignoreCase, bool finding)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["markers"] = new JsonObject { ["withdrawn"] = "Retired", ["superseded"] = "Replaced" },
            ["claim"] = new JsonObject { ["tagIgnoreCase"] = ignoreCase },
        };
        var text = $$"""
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-004 | Retired behavior | Decision | {{status}} |
            ## 9. Traceability Matrix
            | Claim ID | Scenario | Test | Status |
            | --- | --- | --- | --- |
            | B-004 | Old scenario | Missing | {{matrix}} |
            """;
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());
        var tags = tag is null ? [] : new FeatureTagParser().Parse($"@{tag}\nScenario: Old behavior\n", structure);
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(SpechtDocument.Parse(text, "src/sample/.spec/README.md"))
            .WithFeatureFiles(new FeatureFileFixture().WithTags(tags));
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(feature);
        using var services = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();
        var rule = ((IEnumerable<ISpechtRule>)services.Resolve(typeof(IEnumerable<ISpechtRule>))!).SingleOrDefault(static rule => rule.Id == "SPEC035");

        // When
        rule.Should().NotBeNull("schema 0.2.0 ships the retired-claim rule");
        var findings = rule!.Evaluate(model).ToArray();

        // Then
        findings.Should().HaveCount(finding ? 1 : 0);
        rule.DefaultLevel.Should().Be(SpechtRuleLevel.Off);
        if (finding)
        {
            findings[0].Identifier.Should().Be("B-004");
            findings[0].Line.Should().Be(4);
            findings[0].File.Should().Be(feature.RelativePath);
        }
    }
}
