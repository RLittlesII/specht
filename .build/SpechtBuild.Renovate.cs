using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Rocket.Surgery.Nuke.GithubActions;

[DeclaredGitHubActionsSteps(
    "renovate",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnCronSchedule = "0 10 * * 1",
    Enhancements = [nameof(RenovateMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class SpechtBuild
{
    public static RocketSurgeonGitHubActionsConfiguration RenovateMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration.DetailedTriggers.Add(new RocketSurgeonGitHubActionsWorkflowTrigger
        {
            Kind = RocketSurgeonGitHubActionsTrigger.WorkflowDispatch,
        });

        var job = configuration.Jobs.OfType<RocketSurgeonsGithubActionsJob>().Single();

        job.Steps =
        [
            new CheckoutStep("Checkout") { Uses = CheckoutAction },
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
                    ["DOTNET_BUILD_SDK_VERSION"] = DotNetBuildSdkVersion,
                    ["RENOVATE_REPOSITORIES"] = "${{ github.repository }}",
                    ["RENOVATE_HOST_RULES"] = "[{\"hostType\":\"nuget\",\"matchHost\":\"https://nuget.pkg.github.com/rlittlesii/\","
                        + "\"username\":\"${{ github.actor }}\",\"password\":\"${{ github.token }}\"}]",
                },
                With = new Dictionary<string, string>
                {
                    ["token"] = "${{ secrets.RENOVATE_TOKEN }}",
                    ["configurationFile"] = ".github/renovate-global.json",
                    ["docker-cmd-file"] = ".github/renovate-entrypoint.sh",
                    ["docker-user"] = "root",
                    ["additional-env-list"] = "DOTNET_BUILD_SDK_VERSION,DOTNET_SDK_VERSION",
                },
            },
        ];

        configuration.Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
        {
            Contents = GitHubActionsPermission.Read,
            Packages = GitHubActionsPermission.Read,
        };

        return configuration;
    }
}
