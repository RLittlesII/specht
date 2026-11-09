using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Rocket.Surgery.Nuke.GithubActions;
using Serilog;

[DeclaredGitHubActionsSteps(
    "mirror",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushBranches = ["main"],
    OnPushIncludePaths =
    [
        "**/.issue/**",
        "epics/**/epic.md",
        ".github/mirror/**",
        ".github/actions/mirror/**",
        ".agents/skills/specht-conventions/scripts/**",
        ".github/workflows/mirror.yml",
    ],
    InvokedTargets = [nameof(Mirror)],
    Enhancements = [nameof(MirrorMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class SpechtBuild
{
    private Target Mirror => definition => definition
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
        checkout.Uses = CheckoutAction;
        checkout.FetchDepth = 0;
        var mirror = job.Steps.OfType<RunStep>().Single(static step => step.Id == "mirror");
        job.Steps[job.Steps.IndexOf(mirror)] = new UsingStep("Mirror")
        {
            Id = "mirror",
            Uses = "./.github/actions/mirror",
            With = new Dictionary<string, string>
            {
                ["token"] = "${{ github.token }}",
                ["script"] = ".github/mirror/mirror_items.py",
                ["before"] = "${{ github.event.before }}",
                ["after"] = "${{ github.event.after }}",
                ["backfill"] = "${{ inputs.backfill || false }}",
            },
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
