using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Scenarios;
using Specht.Analysis.TestEvidence;
using Specht.Analysis.Tests.Bindings;
using Specht.Analysis.Tests.TestEvidence;

namespace Specht.Analysis.Tests.Scenarios;

[Trait("Tier", "Unit")]
public sealed class ScenarioEvidenceBridgeUnitTests
{
    [Fact]
    public void LocalClaimTag_WhenMapped_ShouldResolveThroughTheCodeBindingIdentityContract()
    {
        // Given
        var resolver = Resolver();
        var expected = resolver.Resolve(new ClaimReference("0201", "F4", "B-001"));
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithClaimTags(new[] { "B-001" });

        // When
        var record = new ScenarioEvidenceBridge(resolver).Map(new[] { scenario }).Single();

        // Then
        var claim = record.GetProperty("claims").EnumerateArray().Single();
        claim.GetProperty("id").GetString().Should().Be("B-001");
        claim.GetProperty("status").GetString().Should().Be("resolved");
        claim.GetProperty("isEligible").GetBoolean().Should().Be(expected.IsEligible!.Value);
        claim.GetProperty("matches").GetArrayLength().Should().Be(expected.Matches.Count);
        var target = claim.GetProperty("target");
        target.GetProperty("kind").GetString().Should().Be("claim");
        target.GetProperty("epic").GetString().Should().Be("0201");
        target.GetProperty("feature").GetString().Should().Be("F4");
        target.GetProperty("id").GetString().Should().Be("B-001");
        target.GetProperty("relativePath").GetString().Should().Be(expected.Target!.RelativePath);
        target.GetProperty("line").GetInt32().Should().Be(expected.Target.Line);
    }

    [Theory]
    [ClassData(typeof(ScenarioClaimTagCases))]
    public void LocalClaimTagThatDoesNotResolve_WhenMapped_ShouldKeepTheResolverStatusWithoutATarget(string tag, string status)
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithClaimTags(new[] { tag });

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        var claim = record.GetProperty("claims").EnumerateArray().Single();
        claim.GetProperty("id").GetString().Should().Be(tag);
        claim.GetProperty("status").GetString().Should().Be(status);
        claim.GetProperty("target").ValueKind.Should().Be(JsonValueKind.Null);
        claim.GetProperty("isEligible").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void OutlineWithTwoDiscoveredCases_WhenMapped_ShouldLinkEachCaseToItsOwnRow()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithRows(new[]
        {
            new ScenarioRow("row-1", "case-1", false),
            new ScenarioRow("row-2", "case-2", false),
        });

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        record.GetProperty("scenarioId").GetString().Should().Be("scenario-1");
        Links(record).Should().Equal(
            ("row-1", "case-1", "resolved", "active"),
            ("row-2", "case-2", "resolved", "active"));
    }

    [Fact]
    public void TwoScenariosOverOneStepMethod_WhenMapped_ShouldKeepEachScenarioLinkedToItsOwnCase()
    {
        // Given
        ScenarioMetadata first = new ScenarioMetadataFixture().WithScenarioId("scenario-1").WithLine(10)
            .WithRows(new[] { new ScenarioRow(null, "case-1", false) });
        ScenarioMetadata second = new ScenarioMetadataFixture().WithScenarioId("scenario-2").WithLine(20)
            .WithRows(new[] { new ScenarioRow(null, "case-2", false) });

        // When
        var records = new ScenarioEvidenceBridge(Resolver()).Map(new[] { first, second });

        // Then
        records.Should().HaveCount(2);
        records[0].GetProperty("scenarioId").GetString().Should().Be("scenario-1");
        records[1].GetProperty("scenarioId").GetString().Should().Be("scenario-2");
        Links(records[0]).Should().Equal((null, "case-1", "resolved", "active"));
        Links(records[1]).Should().Equal((null, "case-2", "resolved", "active"));
    }

    [Fact]
    public void ScenarioWithoutGeneratorInput_WhenMapped_ShouldEmitOneUnresolvedLinkAndKeepItsClaims()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithClaimTags(new[] { "B-001" }).WithRows(Array.Empty<ScenarioRow>())
            .WithGeneratorInputPresent(false);

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        Links(record).Should().Equal((null, null, "unresolved", "active"));
        record.GetProperty("claims").EnumerateArray().Single().GetProperty("status").GetString().Should().Be("resolved");
    }

    [Fact]
    public void NoScenarioMetadata_WhenMapped_ShouldProduceNoRecords()
    {
        // Given
        var bridge = new ScenarioEvidenceBridge(Resolver());

        // When
        var records = bridge.Map(Array.Empty<ScenarioMetadata>());

        // Then
        records.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ScenarioKindCases))]
    public void ScenarioWithoutScopeOverride_WhenMapped_ShouldUseTheFeatureLevelScope(bool outline)
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithFeatureScope(new ScenarioScope("0201", "F3"))
            .WithClaimTags(new[] { "B-001" }).WithRows(Rows(outline));

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        record.GetProperty("scope").GetProperty("epic").GetString().Should().Be("0201");
        record.GetProperty("scope").GetProperty("feature").GetString().Should().Be("F3");
        record.GetProperty("claims")[0].GetProperty("target").GetProperty("feature").GetString().Should().Be("F3");
    }

    [Theory]
    [ClassData(typeof(ScenarioKindCases))]
    public void ScopeOverrideWithoutEpic_WhenMapped_ShouldNotMergeTheFeatureLevelEpic(bool outline)
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithFeatureScope(new ScenarioScope("0201", "F4"))
            .WithScopeOverride(new ScenarioScope(null, "F3")).WithClaimTags(new[] { "B-001" }).WithRows(Rows(outline));

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        record.GetProperty("scope").GetProperty("epic").ValueKind.Should().Be(JsonValueKind.Null);
        record.GetProperty("scope").GetProperty("feature").GetString().Should().Be("F3");
        var claim = record.GetProperty("claims")[0];
        claim.GetProperty("status").GetString().Should().Be("incomplete");
        claim.GetProperty("target").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public void CompleteScopeOverride_WhenMapped_ShouldReplaceEveryFeatureLevelComponent()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithFeatureScope(new ScenarioScope("0999", "F4"))
            .WithScopeOverride(new ScenarioScope("0201", "F3")).WithClaimTags(new[] { "B-001" });

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        record.GetProperty("scope").GetProperty("epic").GetString().Should().Be("0201");
        record.GetProperty("scope").GetProperty("feature").GetString().Should().Be("F3");
        var claim = record.GetProperty("claims")[0];
        claim.GetProperty("status").GetString().Should().Be("resolved");
        claim.GetProperty("target").GetProperty("feature").GetString().Should().Be("F3");
    }

    [Fact]
    public void MappedRecords_WhenCarriedInAnArtifact_ShouldBeAcceptedByTheCommonReader()
    {
        // Given
        ScenarioMetadata linked = new ScenarioMetadataFixture().WithClaimTags(new[] { "B-001" })
            .WithRows(new[] { new ScenarioRow(null, "case-17", false) });
        ScenarioMetadata unlinked = new ScenarioMetadataFixture().WithScenarioId(null).WithLine(20).WithClaimTags(new[] { "B-999" })
            .WithRows(new[] { new ScenarioRow("row-1", null, true) }).WithGeneratorInputPresent(false);
        var records = new ScenarioEvidenceBridge(Resolver()).Map(new[] { linked, unlinked });
        var artifact = JsonNode.Parse(TestEvidenceJson.Valid)!;
        artifact["scenarios"] = new JsonArray(records.Select(static record => JsonNode.Parse(record.GetRawText())).ToArray());

        // When
        var result = new TestEvidenceArtifactReader().Read(artifact.ToJsonString());

        // Then
        result.IsValid.Should().BeTrue();
        result.Artifact!.Scenarios.Should().HaveCount(2);
        result.Artifact.Bindings.Should().ContainSingle();
    }

    [Fact]
    public void OutlineRowsMarkedActiveAndPlanned_WhenMapped_ShouldRecordTheProducerStateOnEachCaseLink()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithRows(new[]
        {
            new ScenarioRow("row-1", "case-1", false),
            new ScenarioRow("row-2", "case-2", true),
        });

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        Links(record).Should().Equal(
            ("row-1", "case-1", "resolved", "active"),
            ("row-2", "case-2", "resolved", "planned"));
    }

    [Fact]
    public void OutlineWithPlannedRows_WhenMapped_ShouldKeepTheSameClaimsAsWithoutThem()
    {
        // Given
        ScenarioMetadata allActive = new ScenarioMetadataFixture().WithClaimTags(new[] { "B-001" }).WithRows(new[]
        {
            new ScenarioRow("row-1", "case-1", false),
            new ScenarioRow("row-2", "case-2", false),
        });
        ScenarioMetadata partlyPlanned = new ScenarioMetadataFixture().WithClaimTags(new[] { "B-001" }).WithRows(new[]
        {
            new ScenarioRow("row-1", "case-1", false),
            new ScenarioRow("row-2", "case-2", true),
        });
        var bridge = new ScenarioEvidenceBridge(Resolver());

        // When
        var active = bridge.Map(new[] { allActive }).Single();
        var mixed = bridge.Map(new[] { partlyPlanned }).Single();

        // Then
        mixed.GetProperty("claims").GetArrayLength().Should().Be(1);
        mixed.GetProperty("claims").GetRawText().Should().Be(active.GetProperty("claims").GetRawText());
    }

    [Fact]
    public void PlannedRowWithoutDiscoveredCase_WhenMapped_ShouldBePlannedAndDistinctFromTheMissingInputLink()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithRows(new[]
        {
            new ScenarioRow("row-1", null, true),
            new ScenarioRow("row-2", null, false),
        }).WithGeneratorInputPresent(false);

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        Links(record).Should().Equal(
            ("row-1", null, "unresolved", "planned"),
            (null, null, "unresolved", "active"));
    }

    [Fact]
    public void ActiveRowWithoutCaseId_WhenGeneratorInputIsPresent_ShouldMapToAnUnresolvedLinkKeepingItsRowId()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithRows(new[] { new ScenarioRow("row-1", null, false) })
            .WithGeneratorInputPresent(true);

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        Links(record).Should().Equal(("row-1", null, "unresolved", "active"));
    }

    [Fact]
    public void PlannedOnlyOutline_WhenGeneratorInputIsMissing_ShouldKeepPlannedLinksAndAppendOneMissingInputLink()
    {
        // Given
        ScenarioMetadata scenario = new ScenarioMetadataFixture().WithRows(new[]
        {
            new ScenarioRow("row-1", null, true),
            new ScenarioRow("row-2", null, true),
        }).WithGeneratorInputPresent(false);

        // When
        var record = new ScenarioEvidenceBridge(Resolver()).Map(new[] { scenario }).Single();

        // Then
        Links(record).Should().Equal(
            ("row-1", null, "unresolved", "planned"),
            ("row-2", null, "unresolved", "planned"),
            (null, null, "unresolved", "active"));
    }

    private static ScenarioRow[] Rows(bool outline) => outline
        ? new[] { new ScenarioRow("row-1", "case-1", false), new ScenarioRow("row-2", "case-2", false) }
        : new[] { new ScenarioRow(null, "case-1", false) };

    private static (string? RowId, string? CaseId, string Status, string State)[] Links(JsonElement record) =>
        record.GetProperty("cases").EnumerateArray().Select(static link => (
            link.GetProperty("rowId").GetString(),
            link.GetProperty("caseId").GetString(),
            link.GetProperty("status").GetString()!,
            link.GetProperty("state").GetString()!)).ToArray();

    private static BindingResolver Resolver()
    {
        BindingFeature scenarios = new BindingFeatureFixture().WithIdentity(new FeatureIdentity("0201", "F4"))
            .WithRelativePath("src/analyzers/Scenarios/.spec/README.md");
        BindingFeature evidence = new BindingFeatureFixture().WithIdentity(new FeatureIdentity("0201", "F3"))
            .WithRelativePath("src/analyzers/TestEvidence/.spec/README.md");
        BindingCatalog catalog = new BindingCatalogFixture().WithFeatures(new[] { scenarios, evidence });
        return new BindingResolver(catalog, new BindingPolicyFixture());
    }
}
