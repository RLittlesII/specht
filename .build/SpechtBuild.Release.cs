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
                !version.StartsWith("0.", StringComparison.Ordinal) || version.Contains('-', StringComparison.Ordinal),
                $"The version '{version}' is below 1.0.0 and carries no prerelease label");

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
            var key = Environment.GetEnvironmentVariable("NUGET_API_KEY");
            Assert.True(!string.IsNullOrWhiteSpace(key), "The release job has no package key.");

            foreach (var package in packages)
            {
                DotNetNuGetPush(settings => settings
                    .SetTargetPath(package)
                    .SetSource("https://api.nuget.org/v3/index.json")
                    .SetApiKey(key)
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
            var prerelease = tag!.Contains('-', StringComparison.Ordinal) ? "--prerelease " : string.Empty;

            ProcessTasks.StartProcess(
                    ToolPathResolver.GetPathExecutable("gh"),
                    $"release create \"{tag}\" {string.Join(' ', packages.Select(static package => $"\"{package}\""))} " +
                    $"--generate-notes --verify-tag {prerelease}--repo \"{repository}\"",
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
                IdToken = GitHubActionsPermission.Write,
            },
            Matrix = [RunnerPool],
            Steps =
            [
                new CheckoutStep("Checkout") { FetchDepth = 0, Clean = true },
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
                new UsingStep("Log in to NuGet")
                {
                    Id = "nuget",
                    Uses = "NuGet/login@v1",
                    With = new Dictionary<string, string>
                    {
                        ["user"] = "${{ vars.NUGET_USER }}",
                    },
                },
                new RunStep("Push release packages")
                {
                    Environment = new Dictionary<string, string>
                    {
                        ["NUGET_API_KEY"] = "${{ steps.nuget.outputs.NUGET_API_KEY }}",
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
