using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Engine;

/// <summary>
/// <see cref="SpechtRunner.Evaluate"/> over a model built in memory and fake rules (ADR-0001 stage B as ADR-0008 amends
/// it, item 0105; <c>0001-F1</c> B-004, C-9; <c>0001-F7</c> B-014): the counts come from the model, the vocabulary of the
/// version the model's schemas keep decides which rules run, which violations are kept, and how many rule ids count as
/// evaluated, the rules run in the order given, and the violations are the ones <see cref="SpechtRunner.Order"/> gives.
/// The report's layouts are the manifest's, by name and in its order, each with its count (<c>0001-F6</c> B-001, B-009, C-7).
/// The manifest's rule settings take an id set <c>off</c> out of the rules run, the violations kept and the count,
/// report an id set <c>warning</c> at warning severity and an id set <c>error</c> at error severity, and leave an id they do
/// not name at the severity the rule gave (<c>0001-F5</c> B-010, B-011, B-046, B-044; item 0014). The report says where
/// the model's schemas came from (<c>0001-F7</c> B-032; item 0092). No tree on disk and no container.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerEvaluateUnitTests
{
    /// <summary>Gets how many specifications in the epics' layout, how many in the features' layout, and how many items a model holds.</summary>
    public static TheoryData<int, int, int> ModelSizes =>
        new()
        {
            { 0, 0, 0 },
            { 2, 1, 3 },
            { 0, 2, 1 },
            { 3, 0, 0 },
        };

    /// <summary>Gets a manifest, then the name, the glob and the specification count of each layout it gives, in its order.</summary>
    public static TheoryData<string, string, string[], string[], int[]> ManifestLayouts =>
        new()
        {
            {
                "three layouts under names the code does not hold, the second holding no specification",
                """
                {
                  "layouts": [
                    { "name": "old-tree", "glob": "archive/**/spec.md" },
                    { "name": "beside-code", "glob": "**/.spec/README.md" },
                    { "name": "documentation", "glob": "docs/**/specification.md" }
                  ]
                }
                """,
                ["old-tree", "beside-code", "documentation"],
                ["archive/**/spec.md", "**/.spec/README.md", "docs/**/specification.md"],
                [2, 0, 1]
            },
            {
                "a manifest leaving its layouts out gives the default manifest's two",
                "{}",
                ["epics", "features"],
                ["epics/**/spec.md", "**/.spec/README.md"],
                [1, 2]
            },
        };

    /// <summary>Gets a vocabulary, the reported ids of each rule given, and how many rule ids count as evaluated.</summary>
    public static TheoryData<string, string[], string[][], int> ReportedIds =>
        new()
        {
            { "no rule evaluates no id", ["FAKE001"], [], 0 },
            { "every id of every rule is in the vocabulary", ["FAKE001", "FAKE002", "FAKE003"], [["FAKE001", "FAKE002"], ["FAKE003"]], 3 },
            { "an evaluated rule's id outside the vocabulary is not counted", ["FAKE001"], [["FAKE001", "FAKE002"]], 1 },
            { "a rule with no id in the vocabulary adds nothing", ["FAKE001"], [["FAKE001"], ["FAKE002", "FAKE003"]], 1 },
            { "entries are counted, not distinct ids, so an id two rules report counts twice", ["FAKE001"], [["FAKE001"], ["FAKE001"]], 2 },
            { "a vocabulary id no rule reports is not counted", ["FAKE001", "FAKE002"], [["FAKE001"]], 1 },
        };

    /// <summary>Gets a manifest under which <c>FAKE001</c> keeps its finding's severity, and the severity that rule gives its finding.</summary>
    public static TheoryData<string, string, SpechtSeverity> KeptSeverities =>
        new()
        {
            { "an id set to error keeps its error finding at error", """{ "rules": { "FAKE001": "error" } }""", SpechtSeverity.Error },
            {
                "an id the rule settings do not name keeps the rule's own severity beside one they lower",
                """{ "rules": { "FAKE002": "warning" } }""",
                SpechtSeverity.Error
            },
            {
                "an id the rule settings do not name keeps the rule's own severity beside one they raise",
                """{ "rules": { "FAKE002": "error" } }""",
                SpechtSeverity.Warning
            },
            { "a manifest leaving its rule settings out keeps the rule's own warning", "{}", SpechtSeverity.Warning },
            { "a manifest leaving its rule settings out keeps the rule's own error", "{}", SpechtSeverity.Error },
        };

    [Theory]
    [MemberData(nameof(ModelSizes))]
    public void AModelBuiltInMemory_WhenEvaluated_ShouldCountItsSpecificationsByLayoutAndItsItems(int inEpics, int inFeatures, int items)
    {
        // Given
        List<FeatureSpec> features =
        [
            .. Enumerable.Repeat(new SpechtLayout("epics", "epics/**/spec.md", new SpechtPathIdentity(1, 2)), inEpics)
                .Concat(Enumerable.Repeat(new SpechtLayout("features", "**/.spec/README.md"), inFeatures))
                .Select(static layout => (FeatureSpec)new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithLayout(layout))),
        ];
        List<ChildItem> children = [.. Enumerable.Range(0, items).Select(static _ => (ChildItem)new ChildItemFixture())];
        SpechtModel model = ModelWithVocabulary().WithFeatures(features).WithItems(children);

        // When
        var report = SpechtRunner.Evaluate(model, []);

        // Then
        (
                report.SpecificationCount,
                report.Layouts.Single(static layout => layout.Layout == "epics").SpecificationCount,
                report.Layouts.Single(static layout => layout.Layout == "features").SpecificationCount,
                report.ItemCount)
            .Should().Be((inEpics + inFeatures, inEpics, inFeatures, items));
    }

    [Theory]
    [MemberData(nameof(ManifestLayouts))]
    public void AModelWhoseManifestGivesItsLayouts_WhenEvaluated_ShouldListEachByItsManifestNameInTheManifestsOrderWithItsSpecificationCount(
        string because,
        string manifest,
        string[] names,
        string[] globs,
        int[] counts)
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest) });
        SchemaVersion version = new SchemaVersionFixture();
        List<FeatureSpec> features =
        [
            .. names
                .Select((name, index) => Enumerable.Repeat(
                    new SpechtLayout(name, globs[index], name == "epics" ? new SpechtPathIdentity(1, 2) : null),
                    counts[index]))
                .Reverse()
                .SelectMany(static layouts => layouts)
                .Select(static layout => (FeatureSpec)new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithLayout(layout))),
        ];
        SpechtModel model = new SpechtModelFixture()
            .WithSchemas(EngineServices.LoadSchemas(fileSystem, "repo", new SchemaVersions([version])))
            .WithFeatures(features);

        // When
        var report = SpechtRunner.Evaluate(model, []);

        // Then
        report.Layouts.Should().Equal(names.Zip(counts, static (name, count) => new SpechtReportLayout(name, count)), because);
    }

    [Fact]
    public void AModelWhoseSchemasKeepAPinnedVersion_WhenEvaluated_ShouldNameThatVersionsNumberInTheReport()
    {
        // Given
        SchemaVersion one = new SchemaVersionFixture();
        SchemaVersion two = new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 2, 0));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", SpechtTree.ManifestPath)] = new("""{ "schemaVersion": "0.2.0" }"""),
            });
        SpechtModel model = new SpechtModelFixture().WithSchemas(EngineServices.LoadSchemas(fileSystem, "repo", new SchemaVersions([one, two])));

        // When
        var report = SpechtRunner.Evaluate(model, []);

        // Then
        report.SchemaVersion.Should().Be(new SemanticVersion(0, 2, 0));
    }

    [Fact]
    public void AModelWhoseSchemasCameFromAnUpstreamCopy_WhenEvaluated_ShouldSayInTheReportTheSchemasCameFromTheUpstreamCopy()
    {
        // Given
        var copy = Encoding.UTF8.GetBytes("{}");
        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(copy));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", SpechtTree.ManifestPath)] = new(
                    $$"""
                    {
                      "spechtSchema": {
                        "location": "https://schemas.example/specht/",
                        "version": "2026.10",
                        "hashes": { "feature": "{{hash}}", "task": "{{hash}}" }
                      }
                    }
                    """),
                [Path.Combine("repo", SpechtTree.FeatureSchemaPath)] = new(copy),
                [Path.Combine("repo", SpechtTree.ItemSchemaPath)] = new(copy),
            });
        SpechtModel model = new SpechtModelFixture().WithSchemas(
            EngineServices.Schemas(fileSystem).Load("repo"));

        // When
        var report = SpechtRunner.Evaluate(model, []);

        // Then
        report.SchemaSource.Should().Be(SpechtSchemaSource.Upstream);
    }

    [Theory]
    [MemberData(nameof(ReportedIds))]
    public void RulesReportingIdsInAndOutOfTheVocabulary_WhenEvaluated_ShouldCountEachReportedIdEntryInTheVocabularyAmongTheRulesThatRan(
        string because,
        string[] vocabulary,
        string[][] reportedIds,
        int expected)
    {
        // Given
        SpechtModel model = ModelWithVocabulary(vocabulary);
        List<ISpechtRule> rules = [.. reportedIds.Select(static ids => new FakeRule(ids[0], ids))];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.RulesEvaluated.Should().Be(expected, because);
    }

    [Fact]
    public void RulesReturningViolationsOutOfOrder_WhenEvaluated_ShouldReportWhatTheRulesReturnedInTheRunnersOrder()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001", "FAKE002");
        SpechtViolation[] first =
        [
            new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Warning).WithFile("b/spec.md").WithLine(5),
            new SpechtViolationFixture().WithRuleId("FAKE001").WithFile("b/spec.md").WithLine(9),
        ];
        SpechtViolation[] second =
        [
            new SpechtViolationFixture().WithRuleId("FAKE002").WithFile("b/spec.md").WithLine(2),
            new SpechtViolationFixture().WithRuleId("FAKE002").WithFile("a/spec.md").WithLine(3),
        ];
        List<ISpechtRule> rules = [new FakeRule("FAKE001", ["FAKE001"], first), new FakeRule("FAKE002", ["FAKE002"], second)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(SpechtRunner.Order([.. first, .. second]));
    }

    [Fact]
    public void ARuleReportingNoIdInTheVocabulary_WhenEvaluated_ShouldNotRunSoAViolationItWouldReturnIsNotReported()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001");
        SpechtViolation inVocabulary = new SpechtViolationFixture().WithRuleId("FAKE001");
        List<ISpechtRule> rules = [new FakeRule("FAKE002", ["FAKE002"], inVocabulary)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().BeEmpty("the rule reports no id in the vocabulary, so the violation it would return is never asked for");
    }

    [Fact]
    public void AViolationWhoseRuleIdIsOutsideTheVocabulary_WhenEvaluated_ShouldBeDroppedAndTheRulesOtherViolationsKept()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001");
        SpechtViolation kept = new SpechtViolationFixture().WithRuleId("FAKE001");
        SpechtViolation dropped = new SpechtViolationFixture().WithRuleId("FAKE002");
        List<ISpechtRule> rules = [new FakeRule("FAKE001", ["FAKE001", "FAKE002"], dropped, kept)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(kept);
    }

    [Fact]
    public void RulesGivenOutOfIdOrder_WhenEvaluated_ShouldRunInTheOrderGivenSoTiedViolationsKeepThatOrder()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001");
        SpechtViolation fromTheFirst = new SpechtViolationFixture().WithRuleId("FAKE001").WithMessage("from the rule given first");
        SpechtViolation fromTheSecond = new SpechtViolationFixture().WithRuleId("FAKE001").WithMessage("from the rule given second");
        List<ISpechtRule> rules = [new FakeRule("FAKE900", ["FAKE001"], fromTheFirst), new FakeRule("FAKE100", ["FAKE001"], fromTheSecond)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(fromTheFirst, fromTheSecond);
    }

    [Fact]
    public void ARuleReportingTwoIdsOneOfThemSetOff_WhenEvaluated_ShouldReportTheOtherIdsViolationsAndCountThatIdAlone()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE002": "off" } }""");
        SpechtViolation kept = new SpechtViolationFixture().WithRuleId("FAKE001");
        SpechtViolation disabled = new SpechtViolationFixture().WithRuleId("FAKE002");
        List<ISpechtRule> rules = [new FakeRule("FAKE001", ["FAKE001", "FAKE002"], disabled, kept)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(kept);
        report.RulesEvaluated.Should().Be(1);
    }

    [Fact]
    public void ARuleWhoseEveryReportedIdIsSetOff_WhenEvaluated_ShouldNotBeAskedForItsViolationsAndShouldLeaveTheCount()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "off", "FAKE002": "off" } }""");
        var asked = false;
        List<ISpechtRule> rules =
        [
            new AskedRule("FAKE001", ["FAKE001", "FAKE002"], () => asked = true),
            new FakeRule("FAKE003", ["FAKE003"]),
        ];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        asked.Should().BeFalse();
        report.RulesEvaluated.Should().Be(1);
    }

    [Fact]
    public void ARuleIdSetToWarning_WhenEvaluated_ShouldReportItsErrorFindingAtWarningSeverityAfterTheErrors()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "warning" } }""");
        SpechtViolation lowered = new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Error).WithFile("a/spec.md");
        SpechtViolation error = new SpechtViolationFixture().WithRuleId("FAKE002").WithSeverity(SpechtSeverity.Error).WithFile("b/spec.md");
        List<ISpechtRule> rules = [new FakeRule("FAKE001", ["FAKE001"], lowered), new FakeRule("FAKE002", ["FAKE002"], error)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(error, lowered with { Severity = SpechtSeverity.Warning });
    }

    [Fact]
    public void ARuleIdSetToError_WhenEvaluated_ShouldReportItsWarningFindingAtErrorSeverity()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "error" } }""");
        SpechtViolation raised = new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Warning);
        List<ISpechtRule> rules = [new FakeRule("FAKE001", ["FAKE001"], raised)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(raised with { Severity = SpechtSeverity.Error });
    }

    [Theory]
    [MemberData(nameof(KeptSeverities))]
    public void ARuleIdSetToErrorOrNotNamed_WhenEvaluated_ShouldReportItsFindingAtTheSeverityTheRuleGaveIt(
        string because,
        string manifest,
        SpechtSeverity severity)
    {
        // Given
        SpechtModel model = ModelUnderManifest(manifest);
        SpechtViolation finding = new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(severity);
        List<ISpechtRule> rules = [new FakeRule("FAKE001", ["FAKE001"], finding)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal([finding], because);
    }

    private static SpechtModelFixture ModelUnderManifest(string manifest)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(
            new HashSet<string>(["FAKE001", "FAKE002", "FAKE003"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest) });

        return new SpechtModelFixture().WithSchemas(EngineServices.LoadSchemas(fileSystem, "repo", new SchemaVersions([version])));
    }

    private static SpechtModelFixture ModelWithVocabulary(params string[] ruleIds)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(ruleIds, StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ManifestPath)] = new("{}") });

        return new SpechtModelFixture().WithSchemas(EngineServices.LoadSchemas(fileSystem, "repo", new SchemaVersions([version])));
    }

    private sealed class AskedRule(string id, IReadOnlyList<string> reportedIds, Action asked) : ISpechtRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => reportedIds;

        public IEnumerable<SpechtViolation> Evaluate(SpechtModel model)
        {
            asked();

            return [];
        }
    }

    private sealed class FakeRule(string id, IReadOnlyList<string> reportedIds, params SpechtViolation[] violations) : ISpechtRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => reportedIds;

        public IEnumerable<SpechtViolation> Evaluate(SpechtModel model) => violations;
    }
}
