using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

/// <summary>
/// NUKE's GitHub Actions generator with one change: the checkout. A pull request builds its head commit rather than
/// the merge GitHub synthesises (B-001), and every run checks out the full history (C-2), which the version is computed
/// from (0055-F5 C-2).
/// </summary>
internal sealed class IntegrationWorkflowAttribute(string name, GitHubActionsImage image, params GitHubActionsImage[] images)
    : GitHubActionsAttribute(name, image, images)
{
    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);
        job.Steps = [.. job.Steps.Select(static step => step is GitHubActionsCheckoutStep ? new CheckoutHeadStep() : step)];
        return job;
    }

    private sealed class CheckoutHeadStep : GitHubActionsStep
    {
        // On a push the expression is empty, and checkout falls back to the pushed commit (B-002).
        public override void Write(CustomFileWriter writer)
        {
            writer.WriteLine("- uses: actions/checkout@v4");
            using (writer.Indent())
            {
                writer.WriteLine("with:");
                using (writer.Indent())
                {
                    writer.WriteLine("ref: ${{ github.event.pull_request.head.sha }}");
                    writer.WriteLine("fetch-depth: 0");
                }
            }
        }
    }
}
