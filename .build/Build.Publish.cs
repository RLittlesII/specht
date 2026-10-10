using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Rocket.Surgery.Nuke.GithubActions;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActionsSteps(
    "publish",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushTags = ["v*"],
    On = [RocketSurgeonGitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(VerifyTag), nameof(Format), nameof(Compile), nameof(Test), nameof(Specht), nameof(Pack)],
    Enhancements = [nameof(PublishMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class Build
{
    private Target VerifyTag => definition => definition
        .DependsOn(Restore)
        .OnlyWhenDynamic(static () => Environment.GetEnvironmentVariable("GITHUB_REF_TYPE") == "tag")
        .Executes(static () =>
        {
            var tag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
            var version = DotNet("nbgv get-version --variable NuGetPackageVersion", RootDirectory, logOutput: false)
                .Single(static line => line.Type == OutputType.Std).Text.Trim();

            Assert.True(
                tag == $"v{version}",
                $"The tag '{tag}' is not 'v{version}', the tag of the version '{version}' computed for this commit");
        });

    public static RocketSurgeonGitHubActionsConfiguration PublishMiddleware(
        RocketSurgeonGitHubActionsConfiguration configuration)
    {
        configuration = Middleware(configuration);

        var buildJob = configuration.Jobs.Cast<RocketSurgeonsGithubActionsJob>()
            .First(static z => z.Name.Equals("build", StringComparison.OrdinalIgnoreCase));

        RunThroughEntryScript(buildJob);

        var packStep = buildJob.Steps.Cast<BaseGitHubActionsStep>().Single(static z => z.Id == "pack");
        buildJob.Steps.Insert(
            buildJob.Steps.IndexOf(packStep) + 1,
            new UploadArtifactStep("Upload the package")
            {
                Name = PackageArtifact,
                Path = $"{PackagePath}/*.nupkg",
                IfNoFilesFound = "error",
                RetentionDays = 1,
                With = new Dictionary<string, string>
                {
                    ["include-hidden-files"] = "true",
                },
            });

        configuration.Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
        {
            Contents = GitHubActionsPermission.Read,
        };

        configuration.Jobs.Add(new RocketSurgeonsGithubActionsJob("publish")
        {
            Needs = ["build"],
            If = "${{ github.event_name == 'push' && startsWith(github.ref, 'refs/tags/v') }}",
            Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
            {
                Contents = GitHubActionsPermission.Read,
                Packages = GitHubActionsPermission.Write,
            },
            Matrix = ["ubuntu-latest"],
            Steps =
            [
                new DownloadArtifactStep("Download the package")
                {
                    Name = PackageArtifact,
                    Path = PackagePath,
                },
                new SetupDotNetStep("Use .NET 10 SDK") { DotNetVersion = "10.0.401" },
                new RunStep("Push")
                {
                    Shell = GithubActionShell.Bash,
                    Environment = new Dictionary<string, string>
                    {
                        ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
                    },
                    Run = $"dotnet nuget push \"{PackagePath}/specht.tool.${{GITHUB_REF_NAME#v}}.nupkg\" " +
                          "--source \"https://nuget.pkg.github.com/rlittlesii/index.json\" --api-key \"$GITHUB_TOKEN\"",
                },
            ],
        });

        return configuration;
    }

    private const string PackageArtifact = "nupkg";

    private const string PackagePath = ".artifacts/nupkg";
}
