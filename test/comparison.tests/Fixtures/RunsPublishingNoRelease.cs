namespace Specht.Comparison.Tests.Workflow;

public sealed class RunsPublishingNoRelease : TheoryData<string, string>
{
    public RunsPublishingNoRelease()
    {
        Add("pull_request", "refs/pull/7/merge");
        Add("push", "refs/heads/main");
        Add("workflow_dispatch", "refs/tags/v2");
        Add("workflow_dispatch", "refs/heads/main");
        Add("push", "refs/tags/release-2");
    }
}
