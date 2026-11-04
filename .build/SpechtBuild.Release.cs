using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Rocket.Surgery.Nuke.GithubActions;
using Serilog;
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
            Assert.True(!string.IsNullOrWhiteSpace(key), "The publish job has no package key.");

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
            var tag = ReleaseTag();
            Assert.True(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GH_TOKEN")),
                "The release job has no GitHub token.");
            var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
            Assert.True(!string.IsNullOrWhiteSpace(repository), "The release job has no repository.");
            var prerelease = tag.Contains('-', StringComparison.Ordinal) ? "--prerelease " : string.Empty;
            var gh = ToolPathResolver.GetPathExecutable("gh");

            if (IsPublishedAtThisCommit(gh, repository!, tag))
            {
                Log.Information("The GitHub Release {Tag} is already published at this commit, so it is kept as it is.", tag);
                return;
            }

            ProcessTasks.StartProcess(
                    gh,
                    $"release create \"{tag}\" --generate-notes --verify-tag {prerelease}--repo \"{repository}\"",
                    RootDirectory)
                .AssertZeroExitCode();
        });

    private static string ReleaseTag()
    {
        var tag = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
        Assert.True(Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true" &&
                    Environment.GetEnvironmentVariable("GITHUB_EVENT_NAME") == "push" &&
                    Environment.GetEnvironmentVariable("GITHUB_REF_TYPE") == "tag" &&
                    tag is { Length: > 1 } && tag.StartsWith('v'),
            "Publishing requires a pushed version tag in GitHub Actions.");
        return tag!;
    }

    private string[] ReleasePackages()
    {
        var version = ReleaseTag()[1..];
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

    private bool IsPublishedAtThisCommit(string gh, string repository, string tag)
    {
        var commit = Environment.GetEnvironmentVariable("GITHUB_SHA");
        if (string.IsNullOrWhiteSpace(commit)
            || Query(gh, $"release view \"{tag}\" --repo \"{repository}\" --json tagName,isDraft") is not { } release
            || Query(gh, $"api \"repos/{repository}/commits/tags/{tag}\" --jq .sha") is not { } tagged)
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(release);
            var root = document.RootElement;
            return tagged.Trim() == commit && root.GetProperty("tagName").GetString() == tag && !root.GetProperty("isDraft").GetBoolean();
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            return false;
        }

        string? Query(string executable, string arguments)
        {
            var process = ProcessTasks.StartProcess(executable, arguments, RootDirectory, logOutput: false);
            process.AssertWaitForExit();
            return process.ExitCode == 0
                ? string.Join('\n', process.Output.Where(static line => line.Type == OutputType.Std).Select(static line => line.Text))
                : null;
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

        configuration.Jobs.Add(new RocketSurgeonsGithubActionsJob("publish")
        {
            Needs = ["build"],
            If = ReleaseCondition,
            Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
            {
                Contents = GitHubActionsPermission.Read,
                IdToken = GitHubActionsPermission.Write,
            },
            Matrix = [GitHubActionsImage.UbuntuLatest.GetValue()],
            Steps =
            [
                Checkout(),
                new DownloadArtifactStep("Download the package")
                {
                    Name = PackageArtifact,
                    Path = PackagePath,
                },
                UseDotNet(),
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
            ],
        });

        configuration.Jobs.Add(new RocketSurgeonsGithubActionsJob("release")
        {
            Needs = ["publish"],
            If = ReleaseCondition,
            Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
            {
                Actions = GitHubActionsPermission.Read,
                Contents = GitHubActionsPermission.Write,
            },
            Matrix = [RunnerPool],
            Steps =
            [
                Checkout(),
                UseDotNet(),
                new RunStep("Create GitHub Release")
                {
                    Environment = new Dictionary<string, string>
                    {
                        ["GH_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
                    },
                    Run = $"./build.cmd --target {nameof(CreateGitHubRelease)}",
                },
                BuildComparator(),
                new RunStep("Request the release comparison")
                {
                    Id = "comparisonRequest",
                    If = ComparatorBuildSucceeded,
                    ContinueOnError = true,
                    Environment = new Dictionary<string, string>
                    {
                        ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
                    },
                    Run = $"dotnet {Comparator} request {RequestPath}",
                },
                new UploadArtifactStep("Upload the comparison request")
                {
                    Name = "comparison-request-${{ github.run_id }}-${{ github.run_attempt }}",
                    Path = RequestPath,
                    IfNoFilesFound = "error",
                    RetentionDays = 1,
                    ContinueOnError = true,
                    With = new Dictionary<string, string>
                    {
                        ["include-hidden-files"] = "true",
                    },
                },
                new RunStep("Attach the comparison report")
                {
                    Id = "comparisonAttach",
                    If = ComparatorBuildSucceeded,
                    ContinueOnError = true,
                    Environment = new Dictionary<string, string>
                    {
                        ["GITHUB_TOKEN"] = "${{ secrets.GITHUB_TOKEN }}",
                    },
                    Run = $"dotnet {Comparator} attach {RequestPath}",
                },
                ComparatorDidNotRun("comparisonAttach", "${{ steps.comparisonAttach.outcome != 'success' }}"),
            ],
        });

        configuration.Jobs.Add(new RocketSurgeonsGithubActionsJob("comparison")
        {
            Needs = ["build"],
            If = ReleaseCondition,
            Permissions = Rocket.Surgery.Nuke.GithubActions.GitHubActionsPermissions.None with
            {
                Actions = GitHubActionsPermission.Read,
                Contents = GitHubActionsPermission.Read,
            },
            Matrix = [RunnerPool],
            Steps =
            [
                new CheckoutStep("Checkout") { Id = "checkout", FetchDepth = 0, Clean = true, PersistCredentials = false, ContinueOnError = true },
                new SetupDotNetStep("Use .NET 10 and 11 SDKs")
                {
                    DotNetVersion = "10.0.401",
                    ContinueOnError = true,
                    With = { ["global-json-file"] = "global.json" },
                },
                BuildComparator(),
                new RunStep("Compare against the previous release")
                {
                    Id = "comparisonCompare",
                    If = ComparatorBuildSucceeded,
                    ContinueOnError = true,
                    Environment = new Dictionary<string, string>
                    {
                        ["GITHUB_TOKEN"] = "${{ github.token }}",
                    },
                    Run = $"dotnet {Comparator} compare {ResultPath}",
                },
                new UploadArtifactStep("Upload the comparison result")
                {
                    Name = "comparison-result-${{ github.run_id }}-${{ github.run_attempt }}",
                    Path = ResultPath,
                    IfNoFilesFound = "error",
                    RetentionDays = 1,
                    ContinueOnError = true,
                    With = new Dictionary<string, string>
                    {
                        ["include-hidden-files"] = "true",
                    },
                },
                ComparatorDidNotRun("comparisonCompare", "${{ always() && steps.comparisonCompare.outcome != 'success' }}"),
            ],
        });

        return configuration;
    }

    private static CheckoutStep Checkout() => new("Checkout") { Id = "checkout", FetchDepth = 0, Clean = true, PersistCredentials = false };

    private static SetupDotNetStep UseDotNet() => new("Use .NET 10 and 11 SDKs")
    {
        DotNetVersion = "10.0.401",
        With = { ["global-json-file"] = "global.json" },
    };

    private static RunStep BuildComparator() => new("Build the comparator from source")
    {
        Id = "comparatorBuild",
        If = "${{ steps.checkout.outcome == 'success' }}",
        Run = $"dotnet publish .performance/comparison/comparison.csproj -c Release -f net10.0 -o {ComparatorPath}",
        ContinueOnError = true,
    };

    private static RunStep ComparatorDidNotRun(string step, string condition) => new("Summarise a comparator that did not run")
    {
        Id = $"{step}Summary",
        If = condition,
        ContinueOnError = true,
        Run = "printf '## Release baseline comparison\\n\\nIncomplete: the comparator did not run. Re-run all jobs to retry.\\n' >> \"$GITHUB_STEP_SUMMARY\"",
    };

    private const string ReleaseCondition = "${{ github.event_name == 'push' && startsWith(github.ref, 'refs/tags/v') }}";

    private const string PackageArtifact = "nupkg";

    private const string PackagePath = ".artifacts/nupkg";

    private const string ComparatorBuildSucceeded = "${{ steps.comparatorBuild.outcome == 'success' }}";

    private const string ComparatorPath = ".artifacts/comparator";

    private const string Comparator = ComparatorPath + "/specht.comparison.dll";

    private const string RequestPath = ".artifacts/benchmarks/comparison/request";

    private const string ResultPath = ".artifacts/benchmarks/comparison/result";
}
