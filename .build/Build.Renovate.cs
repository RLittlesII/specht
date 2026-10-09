using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Rocket.Surgery.Nuke.GithubActions;

// 0055-F4: the self-hosted Renovate run, generated into .github/workflows/renovate.yml and committed (C-8; 0055-F2 C-1).
// B-015: before 6am on Monday, America/Chicago (A-1) - 10:00 UTC - and on demand.
[GitHubActionsSteps(
    "renovate",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnCronSchedule = "0 10 * * 1",
    Enhancements = [nameof(RenovateMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class Build
{
    public static RocketSurgeonGitHubActionsConfiguration RenovateMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration.DetailedTriggers.Add(new RocketSurgeonGitHubActionsWorkflowTrigger
        {
            Kind = RocketSurgeonGitHubActionsTrigger.WorkflowDispatch,
        });

        var job = configuration.Jobs.OfType<RocketSurgeonsGithubActionsJob>().Single();

        // B-016: Renovate runs in a container on the runner, so the SDK global.json pins is installed inside it, by
        // the entrypoint, before Renovate starts and so before any post-upgrade task.
        // C-8: pull requests are opened with the stored token, so they start the checks they wait on.
        job.Steps =
        [
            new CheckoutStep("Checkout"),
            new RunStep("Read the pinned .NET SDK")
            {
                Shell = GithubActionShell.Bash,
                Run = """echo "DOTNET_SDK_VERSION=$(jq -r .sdk.version global.json)" >> "$GITHUB_ENV" """,
            },
            new UsingStep("Renovate")
            {
                Uses = "renovatebot/github-action@v46.3.7",
                Environment = new Dictionary<string, string>
                {
                    ["RENOVATE_REPOSITORIES"] = "${{ github.repository }}",
                },
                With = new Dictionary<string, string>
                {
                    ["token"] = "${{ secrets.RENOVATE_TOKEN }}",
                    ["configurationFile"] = ".github/renovate-global.json",
                    ["docker-cmd-file"] = ".github/renovate-entrypoint.sh",
                    ["docker-user"] = "root",
                    ["additional-env-list"] = "DOTNET_SDK_VERSION",
                },
            },
        ];

        configuration.Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
        {
            Contents = GitHubActionsPermission.Read,
        };

        return configuration;
    }
}
