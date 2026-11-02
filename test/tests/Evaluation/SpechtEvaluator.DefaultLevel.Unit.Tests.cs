using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Evaluation;

/// <summary>Own off levels use the same selection, stamping and counting as manifest off (0001-F5 B-061, B-062).</summary>
[Trait("Tier", "Unit")]
public sealed class SpechtEvaluatorDefaultLevelUnitTests
{
    [Theory]
    [ClassData(typeof(OffRuleSettingsData))]
    public void ARuleWhoseOwnLevelIsOff_WhenEvaluated_ShouldRunReportAndCountOnlyWhenTheManifestRaisesItsId(
        string because,
        string manifest,
        string id,
        bool expectedRun,
        string[] expectedIds,
        SpechtSeverity expectedSeverity)
    {
        // Given
        SpechtModel model = Model(manifest, "FAKE001", "FAKE002", "FAKE003");
        var offRule = new OffRule(id);

        // When
        var report = new SpechtEvaluator([offRule, new EnabledRule("FAKE003")], new SpechtViolationOrder()).Evaluate(model);

        // Then
        offRule.Calls.Should().Be(expectedRun ? 1 : 0, because);
        report.RulesEvaluated.Should().Be(expectedIds.Length + 1, "the enabled control rule still counts; " + because);
        report.Violations.Select(static violation => violation.RuleId).Should().Equal(expectedIds, because);
        report.Violations.Select(static violation => violation.Severity)
            .Should().Equal(Enumerable.Repeat(expectedSeverity, expectedIds.Length), because);
    }

    [Fact]
    public void TwoRulesUnderTheSameId_WhenOnlyOnesOwnLevelIsOff_ShouldRunAndCountTheEnabledRuleAlone()
    {
        // Given
        SpechtModel model = Model("{}", "FAKE001");
        var offRule = new OffRule("FAKE001");

        // When
        var report = new SpechtEvaluator([offRule, new EnabledRule("FAKE001")], new SpechtViolationOrder()).Evaluate(model);

        // Then
        offRule.Calls.Should().Be(0);
        report.RulesEvaluated.Should().Be(1);
        report.Violations.Should().BeEmpty();
    }

    private static SpechtModel Model(string manifest, params string[] vocabulary)
    {
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(vocabulary, StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpechtTree.ManifestPath)] = new(manifest) });

        return new SpechtModelFixture().WithSchemas(EngineServices.ReadSchemas(fileSystem, "repo", new SchemaVersions([version])));
    }

    private sealed class OffRule(string id) : ISpechtRule
    {
        public int Calls { get; private set; }

        public string Id => id;

        public SpechtRuleLevel DefaultLevel => SpechtRuleLevel.Off;

        public IEnumerable<SpechtFinding> Evaluate(SpechtModel model)
        {
            Calls++;

            return [new SpechtFindingFixture()];
        }
    }

    private sealed class EnabledRule(string id) : ISpechtRule
    {
        public string Id => id;

        public IEnumerable<SpechtFinding> Evaluate(SpechtModel model) => [];
    }
}
