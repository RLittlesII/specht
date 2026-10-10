using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// <see cref="SpecCheckRunner.Evaluate"/> over a model built in memory and fake rules (ADR-0001 stage B as ADR-0008 amends
/// it, item 0105; <c>0001-F1</c> B-004, C-9; <c>0001-F7</c> B-014): the counts come from the model, the vocabulary of the
/// version the model's schemas keep decides which rules run, which violations are kept and how many rule ids count as
/// evaluated, the rules run in the order given, and the violations are the ones <see cref="SpecCheckRunner.Order"/> gives.
/// No tree on disk and no container.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecCheckRunnerEvaluateUnitTests
{
    /// <summary>Gets how many legacy specifications, co-located specifications and items a model holds.</summary>
    public static TheoryData<int, int, int> ModelSizes =>
        new()
        {
            { 0, 0, 0 },
            { 2, 1, 3 },
            { 0, 2, 1 },
            { 3, 0, 0 },
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

    [Theory]
    [MemberData(nameof(ModelSizes))]
    public void AModelBuiltInMemory_WhenEvaluated_ShouldCountItsSpecificationsByLayoutAndItsItems(int legacy, int coLocated, int items)
    {
        // Given
        List<FeatureSpec> features =
        [
            .. Enumerable.Repeat(new SpecLayout("legacy", "epics/**/spec.md"), legacy)
                .Concat(Enumerable.Repeat(new SpecLayout("coLocated", "**/.spec/README.md"), coLocated))
                .Select(static layout => (FeatureSpec)new FeatureSpecFixture().WithLocation(new SpecLocationFixture().WithLayout(layout))),
        ];
        List<ChildItem> children = [.. Enumerable.Range(0, items).Select(static _ => (ChildItem)new ChildItemFixture())];
        SpecModel model = ModelWithVocabulary().WithFeatures(features).WithItems(children);

        // When
        var report = SpecCheckRunner.Evaluate(model, []);

        // Then
        (
            report.SpecificationCount,
            report.Layouts.Single(static layout => layout.Layout == "legacy").SpecificationCount,
            report.Layouts.Single(static layout => layout.Layout == "coLocated").SpecificationCount,
            report.ItemCount)
            .Should().Be((legacy + coLocated, legacy, coLocated, items));
    }

    [Fact]
    public void AModelWhoseSchemasKeepAPinnedVersion_WhenEvaluated_ShouldNameThatVersionsNumberInTheReport()
    {
        // Given
        SchemaVersion one = new SchemaVersionFixture();
        SchemaVersion two = new SchemaVersionFixture().WithNumber(2);
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new("""{ "schemaVersion": 2 }"""),
            });
        SpecModel model = new SpecModelFixture().WithSchemas(SpecSchemas.Load(fileSystem, "repo", new SchemaVersions([one, two])));

        // When
        var report = SpecCheckRunner.Evaluate(model, []);

        // Then
        report.SchemaVersion.Should().Be(2);
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
        var report = SpecCheckRunner.Evaluate(model, rules);

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
        var report = SpecCheckRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(SpecCheckRunner.Order([.. first, .. second]));
    }

    [Fact]
    public void ARuleReportingNoIdInTheVocabulary_WhenEvaluated_ShouldNotRunSoAViolationItWouldReturnIsNotReported()
    {
        // Given
        SpecModel model = ModelWithVocabulary("FAKE001");
        SpecViolation inVocabulary = new SpecViolationFixture().WithRuleId("FAKE001");
        List<ISpecRule> rules = [new FakeRule("FAKE002", ["FAKE002"], inVocabulary)];

        // When
        var report = SpecCheckRunner.Evaluate(model, rules);

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
        var report = SpecCheckRunner.Evaluate(model, rules);

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
        var report = SpecCheckRunner.Evaluate(model, rules);

        // Then
        report.Violations.Should().Equal(fromTheFirst, fromTheSecond);
    }

    private static SpecModelFixture ModelWithVocabulary(params string[] ruleIds)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(ruleIds, StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", ".spec", "schema", "spec-structure.schema.json")] = new("{}") });

        return new SpecModelFixture().WithSchemas(SpecSchemas.Load(fileSystem, "repo", new SchemaVersions([version])));
    }

    private sealed class FakeRule(string id, IReadOnlyList<string> reportedIds, params SpecViolation[] violations) : ISpecRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => reportedIds;

        public IEnumerable<SpecViolation> Evaluate(SpecModel model) => violations;
    }
}
