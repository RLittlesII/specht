using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Rocket.Surgery.Nuke.GithubActions;
using Specht.Build;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;

[GitHubActionsSteps(
    "release",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = true,
    OnPushTags = ["v*"],
    On = [RocketSurgeonGitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(Release)],
    Enhancements = [nameof(ReleaseMiddleware)]
)]
[SuppressMessage("Design", "RSA2002:Private members should appear after non-private members", Justification = "Build")]
internal sealed partial class SpechtBuild
{
    private Target Release => definition => definition
        .DependsOn(VerifyTag)
        .DependsOn(Pack);

    private Target VerifyTag => definition => definition
        .DependsOn(Restore)
        .OnlyWhenDynamic(static () => Environment.GetEnvironmentVariable("GITHUB_REF_TYPE") == "tag")
        .Executes(async () =>
        {
            var tag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
            var version = DotNet("nbgv get-version --variable NuGetPackageVersion", RootDirectory, logOutput: false)
                .Single(static line => line.Type == OutputType.Std).Text.Trim();

            Assert.True(
                tag == $"v{version}",
                $"The tag '{tag}' is not 'v{version}', the tag of the version '{version}' computed for this commit");

            Assert.True(
                Run(GitPath, "merge-base --is-ancestor HEAD origin/main"),
                $"The tag '{tag}' is on a commit that is not on 'main', the branch 'origin/main' does not reach it");

            await new SchemaVersionLock(new FileSystem(), RootDirectory).RequireAllLockedAsync();
        });

    private Target PushReleasePackages => definition => definition
        .Executes(() =>
        {
            var packages = ReleasePackages();
            var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            Assert.True(!string.IsNullOrWhiteSpace(token), "The release job has no package token.");

            foreach (var package in packages)
            {
                DotNetNuGetPush(settings => settings
                    .SetTargetPath(package)
                    .SetSource("https://nuget.pkg.github.com/rlittlesii/index.json")
                    .SetApiKey(token)
                    .EnableSkipDuplicate());
            }
        });

    private Target CreateGitHubRelease => definition => definition
        .Executes(() =>
        {
            var packages = ReleasePackages();
            Assert.True(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GH_TOKEN")),
                "The release job has no GitHub token.");
            var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
            Assert.True(!string.IsNullOrWhiteSpace(repository), "The release job has no repository.");
            var tag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");

            ProcessTasks.StartProcess(
                    ToolPathResolver.GetPathExecutable("gh"),
                    $"release create \"{tag}\" {string.Join(' ', packages.Select(static package => $"\"{package}\""))} " +
                    $"--generate-notes --verify-tag --repo \"{repository}\"",
                    RootDirectory)
                .AssertZeroExitCode();
        });

    private string[] ReleasePackages()
    {
        var tag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
        Assert.True(Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" &&
                    Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME") == "push" &&
                    Environment.GetEnvironmentVariable("GITHUB_REF_TYPE") == "tag" &&
                    tag is { Length: > 1 } && tag.StartsWith('v'),
            "Publishing requires a pushed version tag in GitHub Actions.");

        var version = tag![1..];
        var packages = new[]
        {
            PackageFile("src/tool/tool.csproj"),
            PackageFile("src/codeanalysis/codeanalysis.csproj"),
        };
        foreach (var package in packages)
        {
            Assert.True(File.Exists(package), $"Missing release package {Path.GetFileName(package)}.");
        }

        return packages;

        string PackageFile(string project)
        {
            var packageId = XDocument.Load(RootDirectory / project).Descendants("PackageId").Single().Value;
            return PackageDirectory / $"{packageId}.{version}.nupkg";
        }
    }

    public static RocketSurgeonGitHubActionsConfiguration ReleaseMiddleware(
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
            Packages = GitHubActionsPermission.Read,
        };

        configuration.Jobs.Add(new RocketSurgeonsGithubActionsJob("release")
        {
            Needs = ["build"],
            If = "${{ github.event_name == 'push' && startsWith(github.ref, 'refs/tags/v') }}",
            Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
            {
                Contents = GitHubActionsPermission.Write,
                Packages = GitHubActionsPermission.Write,
            },
            Matrix = ["ubuntu-latest"],
            Steps =
            [
                new CheckoutStep("Checkout"),
                new DownloadArtifactStep("Download the package")
                {
                    Name = PackageArtifact,
                    Path = PackagePath,
                },
                new SetupDotNetStep("Use .NET 10 and 11 SDKs")
                {
                    DotNetVersion = "10.0.401",
                    With = { ["global-json-file"] = "global.json" },
                },
                new RunStep("Push release packages")
                {
                    Environment = new Dictionary<string, string>
                    {
                        ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
                    },
                    Run = $"./build.cmd --target {nameof(PushReleasePackages)}",
                },
                new RunStep("Create GitHub Release")
                {
                    Environment = new Dictionary<string, string>
                    {
                        ["GH_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
                    },
                    Run = $"./build.cmd --target {nameof(CreateGitHubRelease)}",
                },
            ],
        });

        return configuration;
    }

    private const string PackageArtifact = "nupkg";

    private const string PackagePath = ".artifacts/nupkg";
}
