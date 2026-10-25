using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Rocket.Surgery.Nuke.GithubActions;
using Serilog;

[GitHubActionsSteps(
    "mirror",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = ["main"],
    OnPushIncludePaths = ["**/.issue/**", "epics/**/epic.md", ".github/mirror/**", ".github/workflows/mirror.yml"],
    InvokedTargets = [nameof(Mirror)],
    Enhancements = [nameof(MirrorMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class SpechtBuild
{
    private Target Mirror => definition => definition
        .DependsOn(MirrorTest)
        .Executes(() =>
        {
            var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
            Assert.True(!string.IsNullOrWhiteSpace(repository), "The mirror job has no repository.");
            Assert.True(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GH_TOKEN")),
                "The mirror job has no GitHub token.");

            var range = string.IsNullOrEmpty(MirrorAfter)
                ? string.Empty
                : $" --before \"{MirrorBefore}\" --after \"{MirrorAfter}\"";
            var backfill = MirrorBackfill ? " --backfill" : string.Empty;
            string arguments = $"-B .github/mirror/mirror_items.py --apply --repo \"{repository}\"{range}{backfill}";

            ProcessTasks.StartProcess(
                    PythonPath,
                    arguments,
                    RootDirectory,
                    logger: static (_, text) => Log.Information(text))
                .AssertZeroExitCode();
        });

    [Parameter("The commit main pointed at before the push the mirror runs for - Default is none")]
    private readonly string? MirrorBefore;

    [Parameter("The commit the push moved main to - Default is none, so no commit counts as pushed")]
    private readonly string? MirrorAfter;

    [Parameter("Post every missing Delivers comment - Default is false")]
    private readonly bool MirrorBackfill;

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

        RunThroughEntryScript(job);

        var checkout = job.Steps.OfType<CheckoutStep>().Single();
        checkout.FetchDepth = 0;
        job.Steps.Insert(
            job.Steps.IndexOf(checkout) + 1,
            new SetupDotNetStep("Use .NET 10 and 11 SDKs") { DotNetVersion = "10.0.401", With = { ["global-json-file"] = "global.json" } });

        job.Steps.OfType<RunStep>().Single(static step => step.Id == "mirror").Environment = new Dictionary<string, string>
        {
            ["GH_TOKEN"] = "${{ github.token }}",
            ["MirrorBefore"] = "${{ github.event.before }}",
            ["MirrorAfter"] = "${{ github.event.after }}",
            ["MirrorBackfill"] = "${{ inputs.backfill || false }}",
        };

        configuration.Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
        {
            Contents = GitHubActionsPermission.Read,
            Issues = GitHubActionsPermission.Write,
            PullRequests = GitHubActionsPermission.Write,
        };

        return configuration;
    }
}
