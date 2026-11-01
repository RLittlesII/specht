using System.Diagnostics;
using AwesomeAssertions;
using YamlDotNet.RepresentationModel;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class CodeAnalysisReleaseIntegrationTests
{
    [Fact]
    public void ReleaseWorkflow_ShouldInvokeNukeTargetsAfterDownloadingPackages()
    {
        var root = FindRepositoryRoot();
        var stream = new YamlStream();
        using var input = File.OpenText(Path.Combine(root, ".github", "workflows", "release.yml"));
        stream.Load(input);
        var workflow = (YamlMappingNode)stream.Documents[0].RootNode;
        var jobs = (YamlMappingNode)workflow["jobs"];
        var release = (YamlMappingNode)jobs["release"];
        var steps = ((YamlSequenceNode)release["steps"]).Cast<YamlMappingNode>().ToArray();

        steps.Should().Contain(static step => step["name"].ToString() == "Checkout");
        steps.Should().Contain(static step => step["name"].ToString().Contains("Download the package", StringComparison.Ordinal));
        steps.Single(static step => step["name"].ToString() == "Push release packages")["run"].ToString().TrimEnd()
            .Should().Be("./build.cmd --target PushReleasePackages");
        steps.Single(static step => step["name"].ToString() == "Create GitHub Release")["run"].ToString().TrimEnd()
            .Should().Be("./build.cmd --target CreateGitHubRelease");
    }

    [Fact]
    public void ReleaseWorkflow_ShouldCheckOutFullHistory()
    {
        var root = FindRepositoryRoot();
        var stream = new YamlStream();
        using var input = File.OpenText(Path.Combine(root, ".github", "workflows", "release.yml"));
        stream.Load(input);
        var workflow = (YamlMappingNode)stream.Documents[0].RootNode;
        var release = (YamlMappingNode)((YamlMappingNode)workflow["jobs"])["release"];
        var checkout = ((YamlSequenceNode)release["steps"]).Cast<YamlMappingNode>()
            .Single(static step => step["name"].ToString() == "Checkout");

        ((YamlMappingNode)checkout["with"])["fetch-depth"].ToString().Should().Be("0");
    }

    [Fact]
    public async Task PushReleasePackages_WhenRunWithoutTagPush_ShouldFailBeforePublication()
    {
        using var fixture = new ReleaseFixture();
        var result = await fixture.RunTarget("PushReleasePackages", "workflow_dispatch");
        result.ExitCode.Should().NotBe(0);
        result.Output.Should().Contain("Publishing requires a pushed version tag");
    }

    [Fact]
    public async Task CreateGitHubRelease_ShouldAttachBothPackedProjects()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        using var fixture = new ReleaseFixture();
        fixture.WritePackages();
        var result = await fixture.RunTarget("CreateGitHubRelease", "push");
        result.ExitCode.Should().Be(0, result.Output);
        var invocation = File.ReadAllText(fixture.GhLog);
        invocation.Should().Contain("specht.tool.1.2.3.nupkg")
            .And.Contain("Specht.CodeAnalysis.1.2.3.nupkg")
            .And.Contain("--generate-notes --verify-tag");
    }

    [Fact]
    public async Task CreateGitHubRelease_WhenTheReleaseIsAlreadyPublishedAtThisCommitWithBothPackages_ShouldKeepItAndSucceed()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        // Given
        using var fixture = new ReleaseFixture();
        fixture.WritePackages();

        // When
        var result = await fixture.RunTarget("CreateGitHubRelease", "push", Published, ReleaseFixture.Commit);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        var invocations = File.ReadAllLines(fixture.GhLog);
        invocations.Should().Contain("release view v1.2.3 --repo RLittlesII/specht --json tagName,isDraft,assets")
            .And.Contain("api repos/RLittlesII/specht/commits/tags/v1.2.3 --jq .sha")
            .And.NotContain(static invocation => invocation.StartsWith("release create", StringComparison.Ordinal));
    }

    [Theory]
    [ClassData(typeof(ReleasesNotPublishedAtThisCommit))]
    public async Task CreateGitHubRelease_WhenNoReleaseIsPublishedAtThisCommitWithBothPackages_ShouldCreateIt(string existing, string tagged)
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        // Given
        using var fixture = new ReleaseFixture();
        fixture.WritePackages();

        // When
        var result = await fixture.RunTarget("CreateGitHubRelease", "push", existing, tagged);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        File.ReadAllLines(fixture.GhLog).Should().ContainSingle(static invocation => invocation.StartsWith("release create v1.2.3", StringComparison.Ordinal))
            .Which.Should().Contain("--generate-notes --verify-tag");
    }

    [Fact]
    public async Task CreateGitHubRelease_WhenCreationFails_ShouldFail()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        // Given
        using var fixture = new ReleaseFixture();
        fixture.WritePackages();

        // When
        var result = await fixture.RunTarget("CreateGitHubRelease", "push", Draft, ReleaseFixture.Commit, createFails: true);

        // Then
        result.ExitCode.Should().NotBe(0);
        File.ReadAllLines(fixture.GhLog).Should().Contain(static invocation => invocation.StartsWith("release create v1.2.3", StringComparison.Ordinal));
    }

    public sealed class ReleasesNotPublishedAtThisCommit : TheoryData<string, string>
    {
        public ReleasesNotPublishedAtThisCommit()
        {
            Add("", ReleaseFixture.Commit);
            Add(Published, "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            Add(Draft, ReleaseFixture.Commit);
            Add("""{"tagName":"v1.2.3","isDraft":false,"assets":[{"name":"specht.tool.1.2.3.nupkg"}]}""", ReleaseFixture.Commit);
            Add("""{"tagName":"v1.2.4","isDraft":false,"assets":[{"name":"specht.tool.1.2.3.nupkg"},{"name":"Specht.CodeAnalysis.1.2.3.nupkg"}]}""",
                ReleaseFixture.Commit);
            Add("not json", ReleaseFixture.Commit);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private sealed class ReleaseFixture : IDisposable
    {
        public ReleaseFixture()
        {
            Directory.CreateDirectory(Path.Combine(Root, ".nuke"));
            Directory.CreateDirectory(Path.Combine(Root, "src", "tool"));
            Directory.CreateDirectory(Path.Combine(Root, "src", "codeanalysis"));
            Directory.CreateDirectory(Path.Combine(Root, "bin"));
            Directory.CreateDirectory(Path.Combine(Root, "runner"));
            File.WriteAllText(Path.Combine(Root, "fixture.slnx"), "<Solution />");
            File.WriteAllText(Path.Combine(Root, ".nuke", "parameters.json"), "{\"Solution\":\"fixture.slnx\"}");
            File.WriteAllText(
                Path.Combine(Root, "src", "tool", "tool.csproj"),
                "<Project><PropertyGroup><PackageId>specht.tool</PackageId></PropertyGroup></Project>");
            File.WriteAllText(
                Path.Combine(Root, "src", "codeanalysis", "codeanalysis.csproj"),
                "<Project><PropertyGroup><PackageId>Specht.CodeAnalysis</PackageId></PropertyGroup></Project>");
            var gh = Path.Combine(Root, "bin", "gh");
            File.WriteAllText(gh, """
                #!/bin/sh
                printf '%s\n' "$*" >> "$INVOCATIONS"
                case "$1 $2" in
                  "release view") [ -n "$EXISTING_RELEASE" ] || exit 1; printf '%s\n' "$EXISTING_RELEASE" ;;
                  "release create") [ "$CREATE_FAILS" != "true" ] || exit 1 ;;
                  "api "*) printf '%s\n' "$TAG_COMMIT" ;;
                esac

                """);
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(gh, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }

        public const string Commit = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        public string Root { get; } = Path.Combine(".artifacts", "release-tests", Guid.NewGuid().ToString("N"));

        public string GhLog => Path.Combine(Root, "gh-arguments.txt");

        public void WritePackages()
        {
            var packages = Path.Combine(Root, ".artifacts", "nupkg");
            Directory.CreateDirectory(packages);
            File.WriteAllText(Path.Combine(packages, "specht.tool.1.2.3.nupkg"), "tool");
            File.WriteAllText(Path.Combine(packages, "Specht.CodeAnalysis.1.2.3.nupkg"), "analysis");
        }

        public async Task<(int ExitCode, string Output)> RunTarget(string target, string eventName, string existingRelease = "",
            string tagCommit = Commit, bool createFails = false)
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = Root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.StartInfo.ArgumentList.Add(typeof(SchemaVersionLock).Assembly.Location);
            process.StartInfo.ArgumentList.Add("--root");
            process.StartInfo.ArgumentList.Add(Path.GetFullPath(Root));
            process.StartInfo.ArgumentList.Add("--target");
            process.StartInfo.ArgumentList.Add(target);
            process.StartInfo.Environment["NUKE_TELEMETRY_OPTOUT"] = "1";
            process.StartInfo.Environment["GITHUB_ACTIONS"] = "true";
            process.StartInfo.Environment["GITHUB_EVENT_NAME"] = eventName;
            process.StartInfo.Environment["GITHUB_REF_TYPE"] = "tag";
            process.StartInfo.Environment["GITHUB_REF_NAME"] = "v1.2.3";
            process.StartInfo.Environment["GITHUB_TOKEN"] = "fixture-token";
            process.StartInfo.Environment["GH_TOKEN"] = "fixture-token";
            process.StartInfo.Environment["GITHUB_REPOSITORY"] = "RLittlesII/specht";
            process.StartInfo.Environment["GITHUB_ENV"] = Path.GetFullPath(Path.Combine(Root, "runner", "environment"));
            process.StartInfo.Environment["GITHUB_OUTPUT"] = Path.GetFullPath(Path.Combine(Root, "runner", "output"));
            process.StartInfo.Environment["GITHUB_STEP_SUMMARY"] = Path.GetFullPath(Path.Combine(Root, "runner", "summary"));
            process.StartInfo.Environment["GITHUB_PATH"] = Path.GetFullPath(Path.Combine(Root, "runner", "path"));
            process.StartInfo.Environment["PATH"] =
                Path.GetFullPath(Path.Combine(Root, "bin")) + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
            process.StartInfo.Environment["INVOCATIONS"] = Path.GetFullPath(GhLog);
            process.StartInfo.Environment["GITHUB_SHA"] = Commit;
            process.StartInfo.Environment["EXISTING_RELEASE"] = existingRelease;
            process.StartInfo.Environment["TAG_COMMIT"] = tagCommit;
            process.StartInfo.Environment["CREATE_FAILS"] = createFails ? "true" : "false";

            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            return (process.ExitCode, await stdout + await stderr);
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    private const string Published =
        """{"tagName":"v1.2.3","isDraft":false,"assets":[{"name":"specht.tool.1.2.3.nupkg"},{"name":"Specht.CodeAnalysis.1.2.3.nupkg"}]}""";

    private const string Draft =
        """{"tagName":"v1.2.3","isDraft":true,"assets":[{"name":"specht.tool.1.2.3.nupkg"},{"name":"Specht.CodeAnalysis.1.2.3.nupkg"}]}""";
}
