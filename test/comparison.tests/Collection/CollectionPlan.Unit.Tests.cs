using AwesomeAssertions;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Collection;

[Trait("Tier", "Unit")]
public sealed class CollectionPlanUnitTests
{
    [Theory]
    [ClassData(typeof(ResolvedCommits))]
    public void ResolvedPair_WhenPlanning_ShouldPinBothCommitsToOneSession(string previous, string current)
    {
        // Given
        ReleasePair pair = new ReleasePairFixture()
            .WithPrevious(new ReleaseRecordFixture().WithId(10).WithCommit(previous))
            .WithCurrent(new ReleaseRecordFixture().WithId(20).WithCommit(current));

        // When
        var plan = CollectionPlan.For(pair, new string('c', 40), "1", "run-1", 1, "session-1", "runner-1", "Specht.Benchmarks.*", "Short");

        // Then
        plan.Should().NotBeNull();
        plan!.PreviousCommit.Should().Be(previous);
        plan.CurrentCommit.Should().Be(current);
        plan.SessionId.Should().Be("session-1");
        plan.RunnerIdentity.Should().Be("runner-1");
    }

    [Theory]
    [ClassData(typeof(UnplannablePairs))]
    public void PairWithoutTwoResolvedCommits_WhenPlanning_ShouldReturnNoPlan(bool hasPredecessor, string? previous, string? current)
    {
        // Given
        ReleasePair pair = new ReleasePairFixture()
            .WithPrevious(hasPredecessor
                ? (ReleaseRecord)new ReleaseRecordFixture().WithId(10).WithCommit(previous).WithResolutionFailure(previous is null ? "Tag unavailable" : null)
                : null)
            .WithCurrent(new ReleaseRecordFixture().WithId(20).WithCommit(current).WithResolutionFailure(current is null ? "Tag unavailable" : null));

        // When
        var plan = CollectionPlan.For(pair, new string('c', 40), "1", "run-1", 1, "session-1", "runner-1", "Specht.Benchmarks.*", "Short");

        // Then
        plan.Should().BeNull();
    }

    [Fact]
    public void SessionId_WhenPlanning_ShouldPlaceTheWorkspaceUnderTheRepositoryRelativeComparisonFolder()
    {
        // Given
        ReleasePair pair = new ReleasePairFixture();

        // When
        var plan = CollectionPlan.For(pair, new string('c', 40), "1", "run-1", 1, "session-7", "runner-1", "Specht.Benchmarks.*", null);

        // Then
        plan!.Workspace.Should().Be(".artifacts/benchmarks/comparison/session-7");
    }

    [Theory]
    [ClassData(typeof(UnsafeSessions))]
    public void UnsafeSessionOrComparator_WhenPlanning_ShouldReturnNoPlan(string sessionId, string comparatorCommit)
    {
        // Given
        ReleasePair pair = new ReleasePairFixture();

        // When
        var plan = CollectionPlan.For(pair, comparatorCommit, "1", "run-1", 1, sessionId, "runner-1", "Specht.Benchmarks.*", "Short");

        // Then
        plan.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(SafeSessions))]
    public void SafeSessionAndComparator_WhenPlanning_ShouldPlan(string sessionId, string comparatorCommit)
    {
        // Given
        ReleasePair pair = new ReleasePairFixture();

        // When
        var plan = CollectionPlan.For(pair, comparatorCommit, "1", "run-1", 1, sessionId, "runner-1", "Specht.Benchmarks.*", "Short");

        // Then
        plan.Should().NotBeNull();
        plan!.SessionId.Should().Be(sessionId);
        plan.ComparatorCommit.Should().Be(comparatorCommit);
    }

    public sealed class ResolvedCommits : TheoryData<string, string>
    {
        public ResolvedCommits()
        {
            Add(new string('1', 40), new string('2', 40));
            Add(new string('a', 64), new string('b', 64));
        }
    }

    public sealed class UnplannablePairs : TheoryData<bool, string?, string?>
    {
        public UnplannablePairs()
        {
            Add(false, null, new string('2', 40));
            Add(true, null, new string('2', 40));
            Add(true, new string('1', 40), null);
            Add(true, "v1.0.0", new string('2', 40));
            Add(true, new string('1', 39), new string('2', 40));
        }
    }

    public sealed class UnsafeSessions : TheoryData<string, string>
    {
        public UnsafeSessions()
        {
            Add(string.Empty, new string('c', 40));
            Add(".", new string('c', 40));
            Add("..", new string('c', 40));
            Add("session/1", new string('c', 40));
            Add("session\\1", new string('c', 40));
            Add("session 1", new string('c', 40));
            Add("sessi\u00f3n-1", new string('c', 40));
            Add("session-1", string.Empty);
            Add("session-1", new string('c', 39));
            Add("session-1", new string('c', 41));
            Add("session-1", new string('g', 40));
            Add("session-1", "v0.3.0");
        }
    }

    public sealed class SafeSessions : TheoryData<string, string>
    {
        public SafeSessions()
        {
            Add("Run_1.attempt-2", new string('c', 40));
            Add("session-1", new string('c', 64));
        }
    }
}
