using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Rocket.Surgery.Nuke.GithubActions;

[GitHubActionsSteps(
    "renovate",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnCronSchedule = "0 10 * * 1",
    Enhancements = [nameof(RenovateMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class SpechtBuild
{
    private Target GenerateWorkflows => static definition => definition
        .Executes(static () =>
        {
            foreach (var workflow in typeof(SpechtBuild).GetCustomAttributes<GitHubActionsStepsAttribute>())
            {
                workflow.ConfigurationFile.DeleteFile();
                ProcessTasks.StartProcess(
                        BuildAssemblyFile,
                        $"--generate-configuration {workflow.Id} --host {workflow.HostName}",
                        RootDirectory)
                    .AssertZeroExitCode();
            }
        });

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
                    ["DOTNET_BUILD_SDK_VERSION"] = BuildHostSdkVersion,
                },
                With = new Dictionary<string, string>
                {
                    ["token"] = "${{ secrets.RENOVATE_TOKEN }}",
                    ["configurationFile"] = ".github/renovate-global.json",
                    ["docker-cmd-file"] = ".github/renovate-entrypoint.sh",
                    ["docker-user"] = "root",
                    ["additional-env-list"] = "|\nDOTNET_SDK_VERSION\nDOTNET_BUILD_SDK_VERSION",
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
