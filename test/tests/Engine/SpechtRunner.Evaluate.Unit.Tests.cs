using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
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
/// not name at the severity the rule gave (<c>0001-F5</c> B-010, B-011, B-043, B-044; item 0014).
/// No tree on disk and no container.
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
    public static TheoryData<string, string, SpecSeverity> KeptSeverities =>
        new()
        {
            { "an id set to error keeps its error finding at error", """{ "rules": { "FAKE001": "error" } }""", SpecSeverity.Error },
            {
                "an id the rule settings do not name keeps the rule's own severity beside one they lower",
                """{ "rules": { "FAKE002": "warning" } }""",
                SpecSeverity.Error
            },
            {
                "an id the rule settings do not name keeps the rule's own severity beside one they raise",
                """{ "rules": { "FAKE002": "error" } }""",
                SpecSeverity.Warning
            },
            { "a manifest leaving its rule settings out keeps the rule's own warning", "{}", SpecSeverity.Warning },
            { "a manifest leaving its rule settings out keeps the rule's own error", "{}", SpecSeverity.Error },
        };

    [Theory]
    [MemberData(nameof(ModelSizes))]
    public void AModelBuiltInMemory_WhenEvaluated_ShouldCountItsSpecificationsByLayoutAndItsItems(int inEpics, int inFeatures, int items)
    {
        // Given
        List<FeatureSpec> features =
        [
            .. Enumerable.Repeat(new SpecLayout("epics", "epics/**/spec.md"), inEpics)
                .Concat(Enumerable.Repeat(new SpecLayout("features", "**/.spec/README.md"), inFeatures))
                .Select(static layout => (FeatureSpec)new FeatureSpecFixture().WithLocation(new SpecLocationFixture().WithLayout(layout))),
        ];
        List<ChildItem> children = [.. Enumerable.Range(0, items).Select(static _ => (ChildItem)new ChildItemFixture())];
        SpecModel model = ModelWithVocabulary().WithFeatures(features).WithItems(children);

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
            new Dictionary<string, MockFileData> { [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new(manifest) });
        SchemaVersion version = new SchemaVersionFixture();
        List<FeatureSpec> features =
        [
            .. names
                .Select((name, index) => Enumerable.Repeat(new SpecLayout(name, globs[index]), counts[index]))
                .Reverse()
                .SelectMany(static layouts => layouts)
                .Select(static layout => (FeatureSpec)new FeatureSpecFixture().WithLocation(new SpecLocationFixture().WithLayout(layout))),
        ];
        SpecModel model = new SpecModelFixture()
            .WithSchemas(SpecSchemas.Load(fileSystem, "repo", new SchemaVersions([version])))
            .WithFeatures(features);

        // When
        var report = SpechtRunner.Evaluate(model, []);

        // Then
        report.Layouts.Should().Equal(names.Zip(counts, static (name, count) => new SpecReportLayout(name, count)), because);
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
                [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new("""{ "schemaVersion": "0.2.0" }"""),
            });
        SpecModel model = new SpecModelFixture().WithSchemas(SpecSchemas.Load(fileSystem, "repo", new SchemaVersions([one, two])));

        // When
        var report = SpechtRunner.Evaluate(model, []);

        // Then
        report.SchemaVersion.Should().Be(new SemanticVersion(0, 2, 0));
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
        SpecModel model = ModelWithVocabulary(vocabulary);
        List<ISpecRule> rules = [.. reportedIds.Select(static ids => new FakeRule(ids[0], ids))];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.RulesEvaluated.Should().Be(expected, because);
    }

    [Fact]
    public void RulesReturningViolationsOutOfOrder_WhenEvaluated_ShouldReportWhatTheRulesReturnedInTheRunnersOrder()
    {
        // Given
        SpecModel model = ModelWithVocabulary("FAKE001", "FAKE002");
        SpecViolation[] first =
        [
            new SpecViolationFixture().WithRuleId("FAKE001").WithSeverity(SpecSeverity.Warning).WithFile("b/spec.md").WithLine(5),
            new SpecViolationFixture().WithRuleId("FAKE001").WithFile("b/spec.md").WithLine(9),
        ];
        SpecViolation[] second =
        [
            new SpecViolationFixture().WithRuleId("FAKE002").WithFile("b/spec.md").WithLine(2),
            new SpecViolationFixture().WithRuleId("FAKE002").WithFile("a/spec.md").WithLine(3),
        ];
        List<ISpecRule> rules = [new FakeRule("FAKE001", ["FAKE001"], first), new FakeRule("FAKE002", ["FAKE002"], second)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(SpechtRunner.Order([.. first, .. second]));
    }

    [Fact]
    public void ARuleReportingNoIdInTheVocabulary_WhenEvaluated_ShouldNotRunSoAViolationItWouldReturnIsNotReported()
    {
        // Given
        SpecModel model = ModelWithVocabulary("FAKE001");
        SpecViolation inVocabulary = new SpecViolationFixture().WithRuleId("FAKE001");
        List<ISpecRule> rules = [new FakeRule("FAKE002", ["FAKE002"], inVocabulary)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().BeEmpty("the rule reports no id in the vocabulary, so the violation it would return is never asked for");
    }

    [Fact]
    public void AViolationWhoseRuleIdIsOutsideTheVocabulary_WhenEvaluated_ShouldBeDroppedAndTheRulesOtherViolationsKept()
    {
        // Given
        SpecModel model = ModelWithVocabulary("FAKE001");
        SpecViolation kept = new SpecViolationFixture().WithRuleId("FAKE001");
        SpecViolation dropped = new SpecViolationFixture().WithRuleId("FAKE002");
        List<ISpecRule> rules = [new FakeRule("FAKE001", ["FAKE001", "FAKE002"], dropped, kept)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(kept);
    }

    [Fact]
    public void RulesGivenOutOfIdOrder_WhenEvaluated_ShouldRunInTheOrderGivenSoTiedViolationsKeepThatOrder()
    {
        // Given
        SpecModel model = ModelWithVocabulary("FAKE001");
        SpecViolation fromTheFirst = new SpecViolationFixture().WithRuleId("FAKE001").WithMessage("from the rule given first");
        SpecViolation fromTheSecond = new SpecViolationFixture().WithRuleId("FAKE001").WithMessage("from the rule given second");
        List<ISpecRule> rules = [new FakeRule("FAKE900", ["FAKE001"], fromTheFirst), new FakeRule("FAKE100", ["FAKE001"], fromTheSecond)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(fromTheFirst, fromTheSecond);
    }

    [Fact]
    public void ARuleReportingTwoIdsOneOfThemSetOff_WhenEvaluated_ShouldReportTheOtherIdsViolationsAndCountThatIdAlone()
    {
        // Given
        SpecModel model = ModelUnderManifest("""{ "rules": { "FAKE002": "off" } }""");
        SpecViolation kept = new SpecViolationFixture().WithRuleId("FAKE001");
        SpecViolation disabled = new SpecViolationFixture().WithRuleId("FAKE002");
        List<ISpecRule> rules = [new FakeRule("FAKE001", ["FAKE001", "FAKE002"], disabled, kept)];

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
        SpecModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "off", "FAKE002": "off" } }""");
        var asked = false;
        List<ISpecRule> rules =
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
        SpecModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "warning" } }""");
        SpecViolation lowered = new SpecViolationFixture().WithRuleId("FAKE001").WithSeverity(SpecSeverity.Error).WithFile("a/spec.md");
        SpecViolation error = new SpecViolationFixture().WithRuleId("FAKE002").WithSeverity(SpecSeverity.Error).WithFile("b/spec.md");
        List<ISpecRule> rules = [new FakeRule("FAKE001", ["FAKE001"], lowered), new FakeRule("FAKE002", ["FAKE002"], error)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(error, lowered with { Severity = SpecSeverity.Warning });
    }

    [Fact]
    public void ARuleIdSetToError_WhenEvaluated_ShouldReportItsWarningFindingAtErrorSeverity()
    {
        // Given
        SpecModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "error" } }""");
        SpecViolation raised = new SpecViolationFixture().WithRuleId("FAKE001").WithSeverity(SpecSeverity.Warning);
        List<ISpecRule> rules = [new FakeRule("FAKE001", ["FAKE001"], raised)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(raised with { Severity = SpecSeverity.Error });
    }

    [Theory]
    [MemberData(nameof(KeptSeverities))]
    public void ARuleIdSetToErrorOrNotNamed_WhenEvaluated_ShouldReportItsFindingAtTheSeverityTheRuleGaveIt(
        string because,
        string manifest,
        SpecSeverity severity)
    {
        // Given
        SpecModel model = ModelUnderManifest(manifest);
        SpecViolation finding = new SpecViolationFixture().WithRuleId("FAKE001").WithSeverity(severity);
        List<ISpecRule> rules = [new FakeRule("FAKE001", ["FAKE001"], finding)];

        // When
        var report = SpechtRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal([finding], because);
    }

    private static SpecModelFixture ModelUnderManifest(string manifest)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(
            new HashSet<string>(["FAKE001", "FAKE002", "FAKE003"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new(manifest) });

        return new SpecModelFixture().WithSchemas(SpecSchemas.Load(fileSystem, "repo", new SchemaVersions([version])));
    }

    private static SpecModelFixture ModelWithVocabulary(params string[] ruleIds)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(ruleIds, StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new("{}") });

        return new SpecModelFixture().WithSchemas(SpecSchemas.Load(fileSystem, "repo", new SchemaVersions([version])));
    }

    private sealed class AskedRule(string id, IReadOnlyList<string> reportedIds, Action asked) : ISpecRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => reportedIds;

        public IEnumerable<SpecViolation> Evaluate(SpecModel model)
        {
            asked();

            return [];
        }
    }

    private sealed class FakeRule(string id, IReadOnlyList<string> reportedIds, params SpecViolation[] violations) : ISpecRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => reportedIds;

        public IEnumerable<SpecViolation> Evaluate(SpecModel model) => violations;
    }
}
