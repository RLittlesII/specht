using System.Collections.Immutable;
using System.Linq;

namespace Specht.Comparison;

/// <summary>Pins both resolved release commits to one comparison session on one runner.</summary>
public sealed record CollectionPlan(
    string PairDigest,
    string PreviousCommit,
    string CurrentCommit,
    string ComparatorCommit,
    string ComparatorVersion,
    string RunId,
    int Attempt,
    string SessionId,
    string RunnerIdentity,
    string Filter,
    string? Job,
    ImmutableArray<IdentityMapping> Mappings)
{
    public string Workspace => $".artifacts/benchmarks/comparison/{SessionId}";

    public static CollectionPlan? For(ReleasePair pair, string comparatorCommit, string comparatorVersion, string runId, int attempt,
        string sessionId, string runnerIdentity, string filter, string? job, ImmutableArray<IdentityMapping> mappings) =>
        IsSegment(sessionId) && ComparisonJson.IsCommit(comparatorCommit)
        && pair.Previous?.Commit is { } previous && ComparisonJson.IsCommit(previous)
        && pair.Current.Commit is { } current && ComparisonJson.IsCommit(current)
            ? new CollectionPlan(ComparisonJson.Digest(pair), previous, current, comparatorCommit, comparatorVersion, runId, attempt,
                sessionId, runnerIdentity, filter, job, mappings)
            : null;

    private static bool IsSegment(string sessionId) => sessionId is { Length: > 0 } and not "." and not ".."
        && sessionId.All(static c => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-');
}
