using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Rocket.Surgery.Nuke.ContinuousIntegration;
using Rocket.Surgery.Nuke.DotNetCore;
using Rocket.Surgery.Nuke.GithubActions;

// 0055-F2: the integration workflow, generated into .github/workflows/ci.yml and committed (C-1).
// B-001, B-002: a pull request to main and a push to main. B-004: every gate, each through the entry script.
[GitHubActionsSteps(
    "ci",
    GitHubActionsImage.UbuntuLatest,
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = true,
    OnPullRequestBranches = ["main"],
    OnPushBranches = ["main"],
    InvokedTargets = [nameof(Format), nameof(Compile), nameof(Test), nameof(SpecCheck), nameof(Pack)],
    NonEntryTargets =
    [
        nameof(ICIEnvironment.CIEnvironment),
        nameof(ITriggerCodeCoverageReports.GenerateCodeCoverageReportCobertura),
        nameof(IGenerateCodeCoverageBadges.GenerateCodeCoverageBadges),
        nameof(IGenerateCodeCoverageReport.GenerateCodeCoverageReport),
        nameof(IGenerateCodeCoverageSummary.GenerateCodeCoverageSummary),
    ],
    ExcludedTargets =
    [
        nameof(ICanClean.Clean),
        nameof(ICanRestoreWithDotNetCore.DotnetToolRestore),
    ],
    Enhancements = [nameof(ContinuousIntegrationMiddleware)]
)]
internal sealed partial class Build
{
    public static RocketSurgeonGitHubActionsConfiguration ContinuousIntegrationMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration = Middleware(configuration);

        var buildJob = configuration.Jobs.Cast<RocketSurgeonsGithubActionsJob>()
            .First(static z => z.Name.Equals("build", StringComparison.OrdinalIgnoreCase));

        // Nuke appends invoked targets, which puts Format after the test
        // steps. dotnet format needs a restored project graph, not build output,
        // so run it ahead of Compile to fail fast on formatting.
        var steps = buildJob.Steps.Cast<BaseGitHubActionsStep>().ToList();
        var formatStep = steps.Single(static z => z.Id == "format");
        var compileStep = steps.Single(static z => z.Id == "compile");

        buildJob.Steps.Remove(formatStep);
        buildJob.Steps.Insert(buildJob.Steps.IndexOf(compileStep), formatStep);

        // B-004: every gate through the entry script, which bootstraps the build from the local tool manifest
        // (0055-F1 C-3) - never a global NUKE install, never the build assembly directly.
        // The Restore step restores the manifest's tools (0055-F1 B-010); the generator's own restore step, emitted only
        // where a manifest exists, would make the workflow depend on the root it is generated in.
        buildJob.Steps.RemoveAll(static z => z is RunStep { StepName: "Install Nuke Global Tool" or "dotnet tool restore" });
        foreach (var run in buildJob.Steps.OfType<RunStep>().Where(static z => z.Run.Contains("--target ", StringComparison.Ordinal)))
        {
            run.Run = $"./build.cmd {run.Run[run.Run.IndexOf("--target ", StringComparison.Ordinal)..]}";
        }

        // C-2: the pull request's head commit, not GitHub's merge commit; empty on a push, so the pushed commit.
        buildJob.Steps.OfType<CheckoutStep>().Single().Ref = "${{ github.event.pull_request.head.sha }}";

        // B-008: a read-only token.
        configuration.Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
        {
            Contents = GitHubActionsPermission.Read,
        };

        // B-003, B-005, B-010: each image is a matrix leg and its own check; a failure on one leg never cancels
        // the other, so each operating system reports its own result.
        buildJob.FailFast = false;

        return configuration;
    }

    public static RocketSurgeonGitHubActionsConfiguration Middleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        var buildJob = configuration.Jobs.Cast<RocketSurgeonsGithubActionsJob>()
            .First(static z => z.Name.Equals("build", StringComparison.OrdinalIgnoreCase));
        var checkoutStep = buildJob.Steps.OfType<CheckoutStep>().Single();
        // For fetch all
        checkoutStep.FetchDepth = 0;
        buildJob.Steps.InsertRange(
            buildJob.Steps.IndexOf(checkoutStep) + 1,
            [
                new RunStep("Fetch all history for all tags and branches") { Run = "git fetch --prune" },
                new SetupDotNetStep("Use .NET 10 SDK") { DotNetVersion = "10.0.401" }
            ]
        );

        return configuration;
    }

    public static RocketSurgeonGitHubActionsConfiguration DeployMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration = Middleware(configuration);

        var buildJob = configuration.Jobs.Cast<RocketSurgeonsGithubActionsJob>()
            .First(static z => z.Name.Equals("build", StringComparison.OrdinalIgnoreCase));

        var setupDotNetStep = buildJob.Steps.OfType<SetupDotNetStep>().Single();
        buildJob.Steps.InsertRange(
            buildJob.Steps.IndexOf(setupDotNetStep) + 1,
            [
                new UsingStep("Login to GitHub Container Registry")
                {
                    Uses = "docker/login-action@v3",
                    With = new Dictionary<string, string>
                    {
                        ["registry"] = "ghcr.io",
                        ["username"] = "${{ github.actor }}",
                        ["password"] = "${{ secrets.GITHUB_TOKEN }}"
                    }
                },
                new UsingStep("Azure Login")
                {
                    Uses = "azure/login@v2",
                    With = new Dictionary<string, string>
                    {
                        ["client-id"] = "${{ secrets.AZURE_CLIENT_ID }}",
                        ["tenant-id"] = "${{ secrets.AZURE_TENANT_ID }}",
                        ["subscription-id"] = "${{ secrets.AZURE_SUBSCRIPTION_ID }}"
                    }
                }
            ]
        );

        buildJob.Environment["ImageVersion"] = "${{ github.ref_name }}";

        if (configuration.Permissions is not null)
        {
            configuration.Permissions.Packages = GitHubActionsPermission.Write;
            configuration.Permissions.IdToken = GitHubActionsPermission.Write;
        }

        return configuration;
    }

    /// <summary>
    /// GitHub Actions related methods.
    /// </summary>
    public static class GitHubActionsTasks
    {
        /// <summary>
        /// A function that evaluates whether this build is running on GitHub Actions.
        /// </summary>
        public static Func<bool> IsRunningOnGitHubActions => static ()
            => NukeBuild.Host is GitHubActions || Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == true.ToString();

        /// <summary>
        /// A function that evaluates whether this build is running on GitHub Actions.
        /// </summary>
        public static Func<bool> IsNotRunningOnGitHubActions => static ()
            => !(NukeBuild.Host is GitHubActions ||
                 Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == true.ToString());
    }

    /// <summary>
    /// Uploads the cobertura files produced by the Test target to Codecov.
    /// Every workflow whose invoked target depends on Test (ci, publish) collects
    /// coverage, so the upload belongs in the shared middleware. Workflows without
    /// a test step (deploy) are left untouched.
    /// </summary>
    private static void AddCodecovUpload(RocketSurgeonsGithubActionsJob buildJob)
    {
        var testStep = buildJob.Steps.Cast<BaseGitHubActionsStep>().SingleOrDefault(static z => z.Id == "test");
        if (testStep is null)
        {
            return;
        }

        buildJob.Steps.Insert(
            buildJob.Steps.IndexOf(testStep) + 1,
            new UsingStep("☂️ Upload coverage to Codecov")
            {
                If = "always()",
                Uses = "codecov/codecov-action@v5",
                With = new Dictionary<string, string>
                {
                    ["token"] = "${{ secrets.CODECOV_TOKEN }}",
                    ["files"] = ".artifacts/coverage/**/*.cobertura.xml",
                    ["fail_ci_if_error"] = "false"
                }
            }
        );
    }
}
