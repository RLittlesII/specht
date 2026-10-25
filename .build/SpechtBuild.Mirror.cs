using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Rocket.Surgery.Nuke.GithubActions;

[GitHubActionsSteps(
    "mirror",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = ["main"],
    OnPushIncludePaths = ["**/.issue/**", "epics/**/epic.md", ".github/mirror/**", ".github/workflows/mirror.yml"],
    Enhancements = [nameof(MirrorMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class SpechtBuild
{
    public static RocketSurgeonGitHubActionsConfiguration MirrorMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration.DetailedTriggers.Add(new RocketSurgeonGitHubActionsWorkflowTrigger
        {
            Kind = RocketSurgeonGitHubActionsTrigger.WorkflowDispatch,
            Inputs = [new GitHubActionsInput("backfill", GitHubActionsInputType.Boolean, "false", "Post every missing Delivers comment")],
        });

        configuration.Concurrency = new RocketSurgeonsGithubActionsConcurrency
        {
            Group = "mirror",
            CancelInProgress = false,
        };

        var job = configuration.Jobs.OfType<RocketSurgeonsGithubActionsJob>().Single();

        job.Steps =
        [
            new CheckoutStep("Checkout") { FetchDepth = 0 },
            new RunStep("Mirror the work items")
            {
                Shell = GithubActionShell.Bash,
                Environment = new Dictionary<string, string>
                {
                    ["GH_TOKEN"] = "${{ github.token }}",
                },
                Run = """
                      python3 .github/mirror/mirror_items.py --apply --repo "${{ github.repository }}" \
                        --before "${{ github.event.before }}" --after "${{ github.event.after }}" \
                        ${{ inputs.backfill && '--backfill' || '' }}
                      """,
            },
        ];

        configuration.Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
        {
            Contents = GitHubActionsPermission.Read,
            Issues = GitHubActionsPermission.Write,
            PullRequests = GitHubActionsPermission.Write,
        };

        return configuration;
    }
}
