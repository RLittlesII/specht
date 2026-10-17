using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Engine;

/// <summary>Own off levels use the same selection, filtering and counting as manifest off (0001-F5 B-061, B-062).</summary>
[Trait("Tier", "Unit")]
public sealed class SpechtRunnerDefaultLevelUnitTests
{
    [Theory]
    [ClassData(typeof(OffRuleSettingsData))]
    public void ARuleWhoseOwnLevelIsOff_WhenEvaluated_ShouldRunReportAndCountOnlyIdsTheManifestRaises(
        string because,
        string manifest,
        string[] reportedIds,
        bool expectedRun,
        string[] expectedIds,
        SpecSeverity expectedSeverity)
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(
            new HashSet<string>(["FAKE001", "FAKE002", "FAKE003"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpecTree.ManifestPath)] = new(manifest) });
        SpecModel model = new SpecModelFixture().WithSchemas(
            EngineServices.LoadSchemas(fileSystem, "repo", new SchemaVersions([version])));
        var calls = 0;
        var offRule = new OffRule(reportedIds, () => calls++);

        // When
        var report = SpechtRunner.Evaluate(model, [offRule, new EnabledRule("FAKE003")]);

        // Then
        calls.Should().Be(expectedRun ? 1 : 0, because);
        report.RulesEvaluated.Should().Be(expectedIds.Length + 1, "the enabled control rule still counts; " + because);
        report.Violations.Select(static violation => violation.RuleId).Should().Equal(expectedIds, because);
        report.Violations.Select(static violation => violation.Severity)
            .Should().Equal(Enumerable.Repeat(expectedSeverity, expectedIds.Length), because);
    }

    [Fact]
    public void TwoFamiliesReportingTheSameId_WhenOnlyOnesOwnLevelIsOff_ShouldRunAndCountTheEnabledFamilyAlone()
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(["FAKE001"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine("repo", SpecTree.ManifestPath)] = new("{}") });
        SpecModel model = new SpecModelFixture().WithSchemas(
            EngineServices.LoadSchemas(fileSystem, "repo", new SchemaVersions([version])));
        var calls = 0;
        var offRule = new OffRule(["FAKE001"], () => calls++);

        // When
        var report = SpechtRunner.Evaluate(model, [offRule, new EnabledRule("FAKE001")]);

        // Then
        calls.Should().Be(0);
        report.RulesEvaluated.Should().Be(1);
        report.Violations.Should().BeEmpty();
    }

    private sealed class OffRule(IReadOnlyList<string> reportedIds, Action evaluated) : ISpecRule
    {
        public string Id => reportedIds[0];

        public IReadOnlyList<string> ReportedIds => reportedIds;

        public SpecRuleLevel DefaultLevel => SpecRuleLevel.Off;

        public IEnumerable<SpecViolation> Evaluate(SpecModel model)
        {
            evaluated();
            return reportedIds.Select(static id => (SpecViolation)new SpecViolationFixture().WithRuleId(id));
        }
    }

    private sealed class EnabledRule(string id) : ISpecRule
    {
        public string Id => id;

        public IReadOnlyList<string> ReportedIds => [Id];

        public IEnumerable<SpecViolation> Evaluate(SpecModel model) => [];
    }
}
