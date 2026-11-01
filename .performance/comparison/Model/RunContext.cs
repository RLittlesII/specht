using System.Collections.Generic;
using System.Globalization;

namespace Specht.Comparison;

/// <summary>The workflow run a comparison step executes in, read from the Actions environment.</summary>
public sealed record RunContext(string EventName, string Ref, string Repository, string RunId, int Attempt, string Sha)
{
    public static RunContext? From(IReadOnlyDictionary<string, string> environment) =>
        environment.TryGetValue("GITHUB_EVENT_NAME", out var eventName) && environment.TryGetValue("GITHUB_REF", out var reference)
        && environment.TryGetValue("GITHUB_REPOSITORY", out var repository) && environment.TryGetValue("GITHUB_RUN_ID", out var runId)
        && environment.TryGetValue("GITHUB_RUN_ATTEMPT", out var attemptText) && environment.TryGetValue("GITHUB_SHA", out var sha)
        && int.TryParse(attemptText, NumberStyles.None, CultureInfo.InvariantCulture, out var attempt)
            ? new RunContext(eventName, reference, repository, runId, attempt, sha)
            : null;
}
