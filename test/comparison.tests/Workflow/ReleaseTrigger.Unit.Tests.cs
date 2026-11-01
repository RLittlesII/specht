using AwesomeAssertions;
using Specht.Comparison.Tests.Reporting;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Unit")]
public sealed class ReleaseTriggerUnitTests
{
    [Fact]
    public void TagPush_WhenTriggerChecked_ShouldStartTheComparison()
    {
        // Given
        RunContext context = new RunContextFixture();

        // When
        var started = ReleaseTrigger.IsReleasePublication(context);

        // Then
        started.Should().BeTrue();
    }

    [Theory]
    [ClassData(typeof(RunsPublishingNoRelease))]
    public void RunThatPublishesNoRelease_WhenTriggerChecked_ShouldNotStartTheComparison(string eventName, string reference)
    {
        // Given
        RunContext context = new RunContextFixture().WithEventName(eventName).WithRef(reference);

        // When
        var started = ReleaseTrigger.IsReleasePublication(context);

        // Then
        started.Should().BeFalse();
    }

    [Fact]
    public void ActionsEnvironment_WhenRead_ShouldGiveTheRunContext()
    {
        // Given
        var environment = ActionsVariables();

        // When
        var context = RunContext.From(environment);

        // Then
        context.Should().Be((RunContext)new RunContextFixture());
    }

    [Theory]
    [ClassData(typeof(RunVariables))]
    public void ActionsEnvironmentMissingAValue_WhenRead_ShouldGiveNoRunContext(string missing)
    {
        // Given
        var environment = ActionsVariables();
        environment.Remove(missing);

        // When
        var context = RunContext.From(environment);

        // Then
        context.Should().BeNull();
    }

    [Fact]
    public void ReceiptOfThisRunsPublication_WhenValidated_ShouldBeAccepted()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair();
        ComparisonReceipt receipt = new ComparisonReceiptFixture();

        // When
        var rejection = ReleaseTrigger.Validate(receipt, new RunContextFixture(), pair);

        // Then
        rejection.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(MismatchedReceipts))]
    public void ReceiptThatDoesNotMatchThisRun_WhenValidated_ShouldBeRejectedWithAReason(string mismatch, ComparisonReceipt receipt, ReleasePair pair)
    {
        // Given
        RunContext context = new RunContextFixture();

        // When
        var rejection = ReleaseTrigger.Validate(receipt, context, pair);

        // Then
        rejection.Should().NotBeNullOrWhiteSpace(mismatch);
    }

    public sealed class RunVariables : TheoryData<string>
    {
        public RunVariables()
        {
            Add("GITHUB_EVENT_NAME");
            Add("GITHUB_REF");
            Add("GITHUB_REPOSITORY");
            Add("GITHUB_RUN_ID");
            Add("GITHUB_RUN_ATTEMPT");
            Add("GITHUB_SHA");
        }
    }

    public sealed class MismatchedReceipts : TheoryData<string, ComparisonReceipt, ReleasePair>
    {
        public MismatchedReceipts()
        {
            ReleasePair pair = ReportFixtures.Pair();
            Add("format version", new ComparisonReceiptFixture().WithFormatVersion(2), pair);
            Add("repository", new ComparisonReceiptFixture().WithRepository("other/tool"), pair);
            Add("run", new ComparisonReceiptFixture().WithRunId("1233"), pair);
            Add("old attempt", new ComparisonReceiptFixture().WithAttempt(1), pair);
            Add("current commit", new ComparisonReceiptFixture().WithCommit(new string('c', 40)), pair);
            Add("current release", new ComparisonReceiptFixture().WithReleaseId(21), pair);
            Add("pair digest", new ComparisonReceiptFixture().WithPairDigest(new string('f', 64)), pair);
            ReleasePair moved = ReportFixtures.Pair().WithCurrent(new ReleaseRecordFixture().WithId(20).WithTag("v2").WithCommit(new string('b', 40)));
            Add("pair of another commit", new ComparisonReceiptFixture().WithPairDigest(ComparisonJson.Digest(moved)), moved);
        }
    }

    private static Dictionary<string, string> ActionsVariables() => new(StringComparer.Ordinal)
    {
        ["GITHUB_EVENT_NAME"] = "push",
        ["GITHUB_REF"] = "refs/tags/v2",
        ["GITHUB_REPOSITORY"] = "example/tool",
        ["GITHUB_RUN_ID"] = "1234",
        ["GITHUB_RUN_ATTEMPT"] = "2",
        ["GITHUB_SHA"] = new string('a', 40),
    };
}
