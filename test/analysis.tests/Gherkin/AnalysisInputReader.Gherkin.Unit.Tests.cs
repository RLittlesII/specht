using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Analysis.Bindings;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class AnalysisInputReaderGherkinUnitTests
{
    [Fact]
    public void GherkinClaimSource_WhenRead_ShouldBuildTheCatalogueFromClaimFilesAndNotReadAClaimsTable()
    {
        // Given
        var inputs = new GherkinInputs().Replace("feature", "src/other/.spec/README.md", TableRecord);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Policy!.Gherkin.Should().NotBeNull();
        var feature = snapshot.Catalog!.Features.Should().ContainSingle().Subject;
        feature.Identity.Should().Be(new FeatureIdentity(null, "arithmetic"));
        feature.Claims.Select(static claim => claim.Id).Should().Equal("REQ-FIX-001");
    }

    [Fact]
    public void NoClaimSource_WhenRead_ShouldKeepTheMarkdownCatalogueAndIgnoreClaimFileSets()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["records"]!["claimFileSets"] = JsonNode.Parse("""[{"glob":"specs/*.feature","feature":"cap:read"}]""");
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Policy!.Gherkin.Should().BeNull();
        snapshot.Catalog!.Features.Single().Claims.Select(static claim => claim.Id).Should().Equal("need/7");
        snapshot.Catalog.Features.Single().Claims.Single().Element.Should().BeNull();
        snapshot.Catalog.Findings.Should().BeEmpty();
    }

    [Fact]
    public void MarkdownRecordWithTheCurrentLifecycleValue_WhenRead_ShouldFailNamingTheRecord()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var index = inputs.FindIndex(static input => input.Kind == "feature");
        inputs[index] = inputs[index] with { Text = inputs[index].Text!.Replace("spec_status: draft", "spec_status: current") };

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Failures.Should().Equal(new AnalysisInputFailure("src/read/.spec/README.md",
            "Required consumer input is missing, invalid or incompatible with its explicit mappings."));
    }

    [Fact]
    public void TwoClaimFileSets_WhenRead_ShouldAssignEachFilesClaimsToItsSetsFeature()
    {
        // Given
        var inputs = new GherkinInputs().Merge("""
            {"codeAnalysis":{"features":[{"feature":"arithmetic"},{"feature":"billing"}],
             "records":{"claimFileSets":[
               {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic"},
               {"glob":".spec/features/billing/*.feature","feature":"billing"}]}}}
            """).Replace("scenario", ".spec/features/billing/billing.feature",
            "@spec.feature:billing\nFeature: Billing\n\n  @REQ-FIX-002\n  Scenario: Invoice\n    Given x\n");

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Catalog!.Features.Single(static feature => feature.Identity.Feature == "arithmetic").Claims
            .Select(static claim => claim.Id).Should().Equal("REQ-FIX-001");
        snapshot.Catalog.Features.Single(static feature => feature.Identity.Feature == "billing").Claims
            .Select(static claim => claim.Id).Should().Equal("REQ-FIX-002");
    }

    [Fact]
    public void IgnoredTagCase_WhenAClaimTagIsRead_ShouldKeepTheIdentifierAsAuthored()
    {
        // Given
        var inputs = new GherkinInputs().Merge("""{"claim":{"tagIgnoreCase":true}}""")
            .ClaimFile("Feature: Arithmetic\n\n  @req-fix-001\n  Scenario: Add\n    Given x\n");

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Catalog!.Features.Single().Claims.Select(static claim => claim.Id).Should().Equal("req-fix-001");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FeatureRecordWithoutAClaimsTable_WhenRead_ShouldReportNoInputFailure(bool withRecord)
    {
        // Given
        var inputs = new GherkinInputs().Replace("feature", GherkinInputs.RecordPath, Record);
        if (withRecord)
        {
            inputs.Merge("""
                {"codeAnalysis":{"records":{"claimFileSets":[
                  {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic","record":"src/arithmetic/.spec/README.md"}]}}}
                """);
        }

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Failures.Should().BeEmpty();
        snapshot.Available.Should().BeTrue();
    }

    [Fact]
    public void SameInputsUnderDifferentSelectedProjects_WhenRead_ShouldGiveEqualCatalogues()
    {
        // Given
        var first = new GherkinInputs();
        var second = new GherkinInputs().Merge("""
            {"codeAnalysis":{"projects":[{"path":"other/Other.csproj","targetFrameworks":["net10.0"]}],"enforcement":[]}}
            """);

        // When
        var one = Read(first);
        var other = Read(second);

        // Then
        one.Available.Should().BeTrue();
        other.Available.Should().BeTrue();
        other.Catalog!.Features.Should().BeEquivalentTo(one.Catalog!.Features);
        new BindingResolver(other.Catalog, other.Policy!.Bindings).Resolve(new ClaimReference(null, "arithmetic", "REQ-FIX-001"))
            .Status.Should().Be(BindingResolution.Resolved);
    }

    [Fact]
    public void ConsumerDeclaringNoEpic_WhenAReferenceNamesNoEpic_ShouldResolveByFeatureAlone()
    {
        // Given
        var snapshot = Read(new GherkinInputs());

        // When
        var result = new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings)
            .Resolve(new ClaimReference(null, "arithmetic", "REQ-FIX-001"));

        // Then
        result.Status.Should().Be(BindingResolution.Resolved);
        result.Target!.Epic.Should().BeNull();
        result.Target.RelativePath.Should().Be(GherkinInputs.ClaimFilePath);
        result.Target.Line.Should().Be(7);
    }

    [Fact]
    public void ReorderedAndRetitledScenarios_WhenRead_ShouldResolveTheSameClaimIdentity()
    {
        // Given
        var before = Read(new GherkinInputs());
        var after = Read(new GherkinInputs().ClaimFile("""
            @spec.feature:arithmetic
            Feature: Arithmetic

              @REQ-FIX-002
              Scenario: Other

              Rule: Moved

                @REQ-FIX-001
                Scenario Outline: Retitled
                  Given <a>

                  Examples:
                    | a |
                    | 1 |
            """));
        var reference = new ClaimReference(null, "arithmetic", "REQ-FIX-001");

        // When
        var original = new BindingResolver(before.Catalog!, before.Policy!.Bindings).Resolve(reference);
        var moved = new BindingResolver(after.Catalog!, after.Policy!.Bindings).Resolve(reference);

        // Then
        moved.Status.Should().Be(BindingResolution.Resolved);
        moved.Target!.Id.Should().Be(original.Target!.Id);
        moved.Target.Feature.Should().Be(original.Target.Feature);
        moved.Target.Line.Should().NotBe(original.Target.Line);
    }

    [Fact]
    public void ClaimIdentifierDeclaredOnTwoScenarios_WhenRead_ShouldReportOneDuplicateFindingPerDeclarationAndKeepBoth()
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile("""
            Feature: Arithmetic

              @REQ-FIX-001
              Scenario: First
                Given x

              @REQ-FIX-001
              Scenario: Second
                Given y
            """);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Catalog!.Features.Single().Claims.Should().HaveCount(2);
        snapshot.Catalog.Findings.Should().Equal(
            new BindingFinding("SPCT0010", GherkinInputs.ClaimFilePath, 4, "Claim REQ-FIX-001 is declared more than once in Feature arithmetic."),
            new BindingFinding("SPCT0010", GherkinInputs.ClaimFilePath, 8, "Claim REQ-FIX-001 is declared more than once in Feature arithmetic."));
        new BindingResolver(snapshot.Catalog, snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, "arithmetic", "REQ-FIX-001"))
            .Status.Should().Be(BindingResolution.Ambiguous);
    }

    [Fact]
    public void FindingsOfSeveralKinds_WhenRead_ShouldBeOrderedByPathThenLineThenId()
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile("""
            Feature: Arithmetic

              @REQ-FIX-001
              Scenario: First
                Given x

              Scenario: Unclaimed
                Given y

              @REQ-FIX-001
              Scenario: Second
                Given z
            """);

        // When
        var findings = Read(inputs).Catalog!.Findings;

        // Then
        findings.Select(static finding => (finding.Line, finding.DiagnosticId)).Should()
            .Equal((4, "SPCT0010"), (7, "SPCT0012"), (11, "SPCT0010"));
    }

    [Fact]
    public void RuleClaimInheritedByItsScenarios_WhenRead_ShouldReportNoDuplicate()
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile("""
            Feature: Arithmetic

              @REQ-FIX-003
              Rule: Addition returns the sum

                Scenario: One
                  Given a

                Scenario: Two
                  Given b
            """);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Catalog!.Features.Single().Claims.Should().ContainSingle();
        snapshot.Catalog.Findings.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "Claim file is missing or unreadable.")]
    [InlineData("Scenario: Orphan\n  Given x\n", "Claim file is not valid Gherkin.")]
    public void ClaimFileThatCannotBeRead_WhenRead_ShouldFailNamingTheFileAndReportNoCatalogue(string? text, string message)
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile(text!);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeFalse();
        snapshot.Catalog.Should().BeNull();
        snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.ClaimFilePath, message));
    }

    [Fact]
    public void ClaimFileInAnotherLanguage_WhenRead_ShouldFailNamingTheFile()
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile("# language: fr\nFonctionnalité: Arithmétique\n");

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.ClaimFilePath,
            "Claim file declares a Gherkin language other than English."));
    }

    [Theory]
    [InlineData("@spec.feature:billing", true)]
    [InlineData("@spec.feature:arithmetic", false)]
    [InlineData("", false)]
    public void FeatureLevelTag_WhenComparedWithItsClaimFileSet_ShouldFailOnlyWhenItNamesAnotherFeature(string tag, bool fails)
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile(tag + "\nFeature: Arithmetic\n\n  @REQ-FIX-001\n  Scenario: Add\n    Given x\n");

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().Be(!fails);
        if (fails)
        {
            snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.ClaimFilePath,
                "Feature or epic tag names a different identity than its claim file set."));
        }
    }

    [Theory]
    [InlineData(false, "draft", "draft", GherkinInputs.RecordPath)]
    [InlineData(false, "approved", "approved", GherkinInputs.RecordPath)]
    [InlineData(true, null, "current", GherkinInputs.ClaimFilePath)]
    public void FeatureLifecycleSource_WhenRead_ShouldTakeTheRecordsStateOrCurrentAndRemainAnEligibleTarget(
        bool noRecord, string? status, string state, string path)
    {
        // Given
        var inputs = new GherkinInputs();
        if (!noRecord)
        {
            inputs.Replace("feature", GherkinInputs.RecordPath, "---\nid: arithmetic\nspec_status: " + status + "\n---\n# Arithmetic\n")
                .Merge("""
                    {"codeAnalysis":{"records":{"claimFileSets":[
                      {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic","record":"src/arithmetic/.spec/README.md"}]}}}
                    """);
        }

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeTrue();
        snapshot.Catalog!.Features.Single().State.Should().Be(state);
        snapshot.Catalog.Features.Single().RelativePath.Should().Be(path);
        var result = new BindingResolver(snapshot.Catalog, snapshot.Policy!.Bindings).Resolve(new ClaimReference(null, "arithmetic", "REQ-FIX-001"));
        result.Target!.FeatureState.Should().Be(state);
        result.IsEligible.Should().BeTrue();
    }

    [Fact]
    public void FeatureRecordWithAnUnknownLifecycleValue_WhenRead_ShouldFailNamingTheRecord()
    {
        // Given
        var inputs = new GherkinInputs()
            .Replace("feature", GherkinInputs.RecordPath, "---\nid: arithmetic\nspec_status: wandering\n---\n# Arithmetic\n")
            .Merge("""
                {"codeAnalysis":{"records":{"claimFileSets":[
                  {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic","record":"src/arithmetic/.spec/README.md"}]}}}
                """);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.RecordPath, "A supplied Feature has an unknown lifecycle value."));
    }

    [Fact]
    public void RecordThatIsNotADeliveredFeatureInput_WhenRead_ShouldFailNamingTheRecordPath()
    {
        // Given
        var inputs = new GherkinInputs().Merge("""
            {"codeAnalysis":{"records":{"claimFileSets":[
              {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic","record":"src/arithmetic/.spec/README.md"}]}}}
            """);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.RecordPath, "Feature record is not a supplied feature input."));
    }

    [Fact]
    public void ClaimFileMatchedByTwoSets_WhenRead_ShouldFailNamingTheFile()
    {
        // Given
        var inputs = new GherkinInputs().Merge("""
            {"codeAnalysis":{"features":[{"feature":"arithmetic"},{"feature":"billing"}],
             "records":{"claimFileSets":[
               {"glob":".spec/features/arithmetic/*.feature","feature":"arithmetic"},
               {"glob":".spec/features/*/*.feature","feature":"billing"}]}}}
            """);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.ClaimFilePath,
            "A claim file matches more than one claim file set."));
    }

    [Theory]
    [ClassData(typeof(InvalidGherkinPolicies))]
    public void InvalidNativeClaimConfiguration_WhenRead_ShouldFailNamingTheManifestWithTheStatedMessage(string patch, string message)
    {
        // Given
        var inputs = new GherkinInputs().Merge(patch);

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeFalse();
        snapshot.Catalog.Should().BeNull();
        snapshot.Failures.Should().Equal(new AnalysisInputFailure(GherkinInputs.ConfigurationPath, message));
    }

    [Theory]
    [InlineData("""{"codeAnalysis":{"records":{"plannedTags":null}}}""", true)]
    [InlineData("""{"codeAnalysis":{"records":{"plannedTags":["@ignore"]}}}""", true)]
    [InlineData("""{"codeAnalysis":{"records":{"plannedTags":[]}}}""", false)]
    public void PlannedTagList_WhenAbsentOrConfigured_ShouldPlanCasesAccordingly(string patch, bool planned)
    {
        // Given
        var inputs = new GherkinInputs().Merge(patch)
            .ClaimFile("Feature: Arithmetic\n\n  @REQ-FIX-001 @ignore\n  Scenario: Add\n    Given x\n");

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Catalog!.Features.Single().Claims.Single().Cases.Should().Equal(new BindingClaimCase(4, planned));
    }

    [Fact]
    public void IgnoredScenario_WhenReadBesideAnUnplannedOne_ShouldKeepTheClaimAndItsRequiredLinks()
    {
        // Given
        var unplanned = Read(new GherkinInputs());
        var planned = Read(new GherkinInputs().ClaimFile(GherkinInputs.Arithmetic.Replace("@REQ-FIX-001", "@REQ-FIX-001 @ignore")));
        var reference = new ClaimReference(null, "arithmetic", "REQ-FIX-001");

        // When
        var kept = new BindingResolver(planned.Catalog!, planned.Policy!.Bindings).Resolve(reference);

        // Then
        kept.Status.Should().Be(BindingResolution.Resolved);
        planned.Catalog!.Features.Single().Claims.Single().Cases.Should().OnlyContain(static item => item.Planned);
        planned.Policy.RequiredLinks(reference).Should().Equal(unplanned.Policy!.RequiredLinks(reference));
        planned.Policy.RequiredLinks(reference).Should().Contain("test");
    }

    [Fact]
    public void ClaimInputsGiven_WhenRead_ShouldLeaveEveryInputUnchangedAndAddNone()
    {
        // Given
        var inputs = new GherkinInputs();
        var before = inputs.Inputs.ToArray();

        // When
        var snapshot = Read(inputs);

        // Then
        snapshot.Available.Should().BeTrue();
        inputs.Inputs.Should().Equal(before);
    }

    private static AnalysisSnapshot Read(GherkinInputs inputs) => Read(inputs.Inputs);

    private static AnalysisSnapshot Read(IReadOnlyList<AnalysisInput> inputs) =>
        new AnalysisInputReader().Read(inputs, TestContext.Current.CancellationToken);

    private const string Record = "---\nid: arithmetic\nspec_status: draft\n---\n# Arithmetic\n";
    private const string TableRecord = "---\nid: arithmetic\nspec_status: draft\n---\n## 3. Acceptance Criteria\n"
        + "| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n| REQ-FIX-777 | A table claim | Agreement | Active |\n";
}
