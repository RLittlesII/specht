using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Evaluation;

/// <summary>
/// <see cref="SpechtEvaluator.Evaluate"/> over a model built in memory and fake rules (ADR-0010 § "The evaluator is pure",
/// item 0164; <c>0001-F1</c> B-004, C-9; <c>0001-F7</c> B-014): the counts come from the model; a rule is selected when its
/// id is in the vocabulary of the version the model's schemas keep and its level is not off, and the count of rules
/// evaluated is the count selected; a rule not selected is never asked for its findings; each selected rule runs once, in
/// the order given; each finding is stamped with its rule's id and with the manifest's level for that id, or the rule's own
/// default level where the manifest names none (<c>0001-F5</c> B-010, B-011, B-044, B-046; item 0014); and the violations
/// come in <see cref="SpechtViolationOrder"/>'s order. The report's layouts are the manifest's, by name and in its order,
/// each with its count (<c>0001-F6</c> B-001, B-009, C-7). The report says where the model's schemas came from
/// (<c>0001-F7</c> B-032; item 0092). No tree on disk and no container.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtEvaluatorUnitTests
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
        List<ChildItem> children = [.. Enumerable.Range(0, items).Select(static _ => (ChildItem) new ChildItemFixture())];
        SpechtModel model = ModelWithVocabulary().WithFeatures(features).WithItems(children);

        // When
        var report = Evaluator().Evaluate(model);

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
            .WithSchemas(EngineServices.ReadSchemas(fileSystem, "repo", new SchemaVersions([version])))
            .WithFeatures(features);

        // When
        var report = Evaluator().Evaluate(model);

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
        SpechtModel model = new SpechtModelFixture().WithSchemas(EngineServices.ReadSchemas(fileSystem, "repo", new SchemaVersions([one, two])));

        // When
        var report = Evaluator().Evaluate(model);

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
            EngineServices.Reader(fileSystem).ReadSchemas("repo", null));

        // When
        var report = Evaluator().Evaluate(model);

        // Then
        report.SchemaSource.Should().Be(SpechtSchemaSource.Upstream);
    }

    [Theory]
    [ClassData(typeof(SelectedRuleCountData))]
    public void RulesWithIdsInAndOutOfTheVocabulary_WhenEvaluated_ShouldCountEachRuleSelected(
        string because,
        string[] vocabulary,
        string[] ruleIds,
        int expected)
    {
        // Given
        SpechtModel model = ModelWithVocabulary(vocabulary);
        ISpechtRule[] rules = [.. ruleIds.Select(static id => new FakeRule(id))];

        // When
        var report = Evaluator(rules).Evaluate(model);

        // Then
        report.RulesEvaluated.Should().Be(expected, because);
    }

    [Fact]
    public void AFindingOfASelectedRule_WhenEvaluated_ShouldBeReportedUnderTheRulesIdWithEveryFieldTheRuleGaveIt()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001");
        SpechtFinding finding = new SpechtFindingFixture()
            .WithFile("src/sample/.spec/README.md")
            .WithLine(7)
            .WithIdentifier("B-002")
            .WithMessage("the claim has no row");
        var rule = new FakeRule("FAKE001", finding with { Expected = new JsonObject { ["rows"] = 1 } });
        SpechtViolation expected = new SpechtViolationFixture()
            .WithRuleId("FAKE001")
            .WithSeverity(SpechtSeverity.Error)
            .WithFile("src/sample/.spec/README.md")
            .WithLine(7)
            .WithIdentifier("B-002")
            .WithMessage("the claim has no row");

        // When
        var report = Evaluator(rule).Evaluate(model);

        // Then
        report.Violations.Should().Equal(expected with { Expected = new JsonObject { ["rows"] = 1 } });
    }

    [Fact]
    public void RulesReturningFindingsOutOfOrder_WhenEvaluated_ShouldReportEachOnceInTheViolationOrder()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001", "FAKE002");
        var warnings = new FakeRule(
            "FAKE001",
            new SpechtFindingFixture().WithFile("b/spec.md").WithLine(9),
            new SpechtFindingFixture().WithFile("b/spec.md").WithLine(5))
        { DefaultLevel = SpechtRuleLevel.Warning };
        var errors = new FakeRule(
            "FAKE002",
            new SpechtFindingFixture().WithFile("b/spec.md").WithLine(2),
            new SpechtFindingFixture().WithFile("a/spec.md").WithLine(3));
        SpechtViolation[] expected =
        [
            new SpechtViolationFixture().WithRuleId("FAKE002").WithFile("a/spec.md").WithLine(3),
            new SpechtViolationFixture().WithRuleId("FAKE002").WithFile("b/spec.md").WithLine(2),
            new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Warning).WithFile("b/spec.md").WithLine(5),
            new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Warning).WithFile("b/spec.md").WithLine(9),
        ];

        // When
        var report = Evaluator(warnings, errors).Evaluate(model);

        // Then
        report.Violations.Should().Equal(expected);
    }

    [Fact]
    public void ARuleWhoseIdIsOutsideTheVocabulary_WhenEvaluated_ShouldNotRunSoAFindingItWouldReturnIsNotReported()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001");
        var outside = new FakeRule("FAKE002", new SpechtFindingFixture());

        // When
        var report = Evaluator(outside).Evaluate(model);

        // Then
        outside.Calls.Should().Be(0, "a rule outside the vocabulary is not selected, so it is never asked for its findings");
        report.Violations.Should().BeEmpty();
        report.RulesEvaluated.Should().Be(0);
    }

    [Fact]
    public void TwoRulesUnderOneIdGivenInAnOrder_WhenEvaluated_ShouldRunEachOnceInTheOrderGivenSoTiedViolationsKeepThatOrder()
    {
        // Given
        SpechtModel model = ModelWithVocabulary("FAKE001");
        var first = new FakeRule("FAKE001", new SpechtFindingFixture().WithMessage("from the rule given first"));
        var second = new FakeRule("FAKE001", new SpechtFindingFixture().WithMessage("from the rule given second"));
        SpechtViolation fromTheFirst = new SpechtViolationFixture().WithRuleId("FAKE001").WithMessage("from the rule given first");
        SpechtViolation fromTheSecond = new SpechtViolationFixture().WithRuleId("FAKE001").WithMessage("from the rule given second");

        // When
        var report = Evaluator(first, second).Evaluate(model);

        // Then
        (first.Calls, second.Calls).Should().Be((1, 1));
        report.Violations.Should().Equal(fromTheFirst, fromTheSecond);
    }

    [Fact]
    public void ARuleWhoseIdIsSetOff_WhenEvaluated_ShouldNotRunAndShouldLeaveTheOtherRulesViolationsAndCount()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "off" } }""");
        var disabled = new FakeRule("FAKE001", new SpechtFindingFixture().WithMessage("from the rule set off"));
        var kept = new FakeRule("FAKE002", new SpechtFindingFixture());
        SpechtViolation expected = new SpechtViolationFixture().WithRuleId("FAKE002");

        // When
        var report = Evaluator(disabled, kept).Evaluate(model);

        // Then
        disabled.Calls.Should().Be(0);
        report.Violations.Should().Equal(expected);
        report.RulesEvaluated.Should().Be(1);
    }

    [Fact]
    public void ARuleIdSetToWarning_WhenEvaluated_ShouldReportItsFindingAtWarningSeverityAfterTheErrors()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "warning" } }""");
        var lowered = new FakeRule("FAKE001", new SpechtFindingFixture().WithFile("a/spec.md"));
        var error = new FakeRule("FAKE002", new SpechtFindingFixture().WithFile("b/spec.md"));
        SpechtViolation fromTheError = new SpechtViolationFixture().WithRuleId("FAKE002").WithSeverity(SpechtSeverity.Error).WithFile("b/spec.md");
        SpechtViolation fromTheLowered = new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Warning).WithFile("a/spec.md");

        // When
        var report = Evaluator(lowered, error).Evaluate(model);

        // Then
        report.Violations.Should().Equal(fromTheError, fromTheLowered);
    }

    [Fact]
    public void ARuleIdSetToError_WhenEvaluated_ShouldReportTheFindingOfARuleWhoseOwnLevelIsWarningAtErrorSeverity()
    {
        // Given
        SpechtModel model = ModelUnderManifest("""{ "rules": { "FAKE001": "error" } }""");
        var raised = new FakeRule("FAKE001", new SpechtFindingFixture()) { DefaultLevel = SpechtRuleLevel.Warning };
        SpechtViolation expected = new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(SpechtSeverity.Error);

        // When
        var report = Evaluator(raised).Evaluate(model);

        // Then
        report.Violations.Should().Equal(expected);
    }

    [Theory]
    [ClassData(typeof(OwnLevelSeverityData))]
    public void ARuleIdSetToErrorOrNotNamed_WhenEvaluated_ShouldReportItsFindingAtTheManifestsLevelElseTheRulesOwnLevel(
        string because,
        string manifest,
        SpechtRuleLevel ownLevel,
        SpechtSeverity severity)
    {
        // Given
        SpechtModel model = ModelUnderManifest(manifest);
        var rule = new FakeRule("FAKE001", new SpechtFindingFixture()) { DefaultLevel = ownLevel };
        SpechtViolation expected = new SpechtViolationFixture().WithRuleId("FAKE001").WithSeverity(severity);

        // When
        var report = Evaluator(rule).Evaluate(model);

        // Then
        report.Violations.Should().Equal([expected], because);
    }

    private static SpechtEvaluator Evaluator(params ISpechtRule[] rules) => new(rules, new SpechtViolationOrder());

    private static SpechtModelFixture ModelUnderManifest(string manifest)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(
            new HashSet<string>(["FAKE001", "FAKE002", "FAKE003"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest) });

        return new SpechtModelFixture().WithSchemas(EngineServices.ReadSchemas(fileSystem, "repo", new SchemaVersions([version])));
    }

    private static SpechtModelFixture ModelWithVocabulary(params string[] ruleIds)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(ruleIds, StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ManifestPath)] = new("{}") });

        return new SpechtModelFixture().WithSchemas(EngineServices.ReadSchemas(fileSystem, "repo", new SchemaVersions([version])));
    }

    private sealed class FakeRule(string id, params SpechtFinding[] findings) : ISpechtRule
    {
        public int Calls { get; private set; }

        public string Id => id;

        public SpechtRuleLevel DefaultLevel { get; init; } = SpechtRuleLevel.Error;

        public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
        {
            Calls++;

            return findings;
        }
    }
}
