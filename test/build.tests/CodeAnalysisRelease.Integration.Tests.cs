using System.Diagnostics;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using YamlDotNet.RepresentationModel;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class CodeAnalysisReleaseIntegrationTests
{
    [Fact]
    public void ReleaseWorkflow_ShouldInvokeNukeTargetsAfterCheckout()
    {
        // Given
        var root = FindRepositoryRoot();
        var stream = new YamlStream();
        using var input = File.OpenText(Path.Combine(root, ".github", "workflows", "release.yml"));
        stream.Load(input);
        var jobs = (YamlMappingNode) ((YamlMappingNode) stream.Documents[0].RootNode)["jobs"];

        // When
        var targets = new[] { ("publish", "Push release packages", "PushReleasePackages"), ("release", "Create GitHub Release", "CreateGitHubRelease") };

        // Then
        jobs.Children.Keys.Select(static key => key.ToString()).Should().Contain("publish");
        foreach (var (job, name, target) in targets)
        {
            var steps = ((YamlSequenceNode) ((YamlMappingNode) jobs[job])["steps"]).Cast<YamlMappingNode>().ToArray();
            steps.Should().Contain(static step => step["name"].ToString() == "Checkout", job);
            steps.Should().ContainSingle(step => step["name"].ToString() == name, job)
                .Which["run"].ToString().TrimEnd().Should().Be($"./build.cmd --target {target}");
        }

        ((YamlSequenceNode) ((YamlMappingNode) jobs["publish"])["steps"]).Cast<YamlMappingNode>()
            .Should().Contain(static step => step["name"].ToString().Contains("Download the package", StringComparison.Ordinal));
    }

    [Fact]
    public void ReleaseWorkflow_ShouldCheckOutFullHistory()
    {
        var root = FindRepositoryRoot();
        var stream = new YamlStream();
        using var input = File.OpenText(Path.Combine(root, ".github", "workflows", "release.yml"));
        stream.Load(input);
        var workflow = (YamlMappingNode) stream.Documents[0].RootNode;
        var release = (YamlMappingNode) ((YamlMappingNode) workflow["jobs"])["release"];
        var checkout = ((YamlSequenceNode) release["steps"]).Cast<YamlMappingNode>()
            .Single(static step => step["name"].ToString() == "Checkout");

        ((YamlMappingNode) checkout["with"])["fetch-depth"].ToString().Should().Be("0");
    }

    [Theory]
    [InlineData("PushReleasePackages")]
    [InlineData("CreateGitHubRelease")]
    public async Task PublicationTarget_WhenRunWithoutTagPush_ShouldFailBeforePublication(string target)
    {
        using var fixture = new ReleaseFixture();
        var (ExitCode, Output) = await fixture.RunTarget(target, "workflow_dispatch");
        ExitCode.Should().NotBe(0);
        Output.Should().Contain("Publishing requires a pushed version tag");
    }

    [Fact]
    public void ReleaseWorkflow_ShouldPushWithTheKeyTheNuGetLoginIssues()
    {
        // Given
        var root = FindRepositoryRoot();
        var stream = new YamlStream();
        using var input = File.OpenText(Path.Combine(root, ".github", "workflows", "release.yml"));
        stream.Load(input);
        var jobs = (YamlMappingNode) ((YamlMappingNode) stream.Documents[0].RootNode)["jobs"];

        // When
        jobs.Children.Keys.Select(static key => key.ToString()).Should().Contain("publish");
        var steps = ((YamlSequenceNode) ((YamlMappingNode) jobs["publish"])["steps"]).Cast<YamlMappingNode>().ToArray();

        // Then
        steps.Select(static step => step.Children.ContainsKey("uses") ? step["uses"].ToString().Split('@')[0] : step["name"].ToString())
            .Should().Equal("actions/checkout", "actions/download-artifact", "actions/setup-dotnet", "NuGet/login", "Push release packages");
        ((YamlMappingNode) steps[1]["with"])["name"].ToString().Should().Be("nupkg");
        steps[3]["name"].ToString().Should().Be("Log in to NuGet");
        steps[3]["uses"].ToString().Should().Be("NuGet/login@v1");
        ((YamlMappingNode) steps[4]["env"])["NUGET_API_KEY"].ToString()
            .Should().Be($"${{{{ steps.{steps[3]["id"]}.outputs.NUGET_API_KEY }}}}");
    }

    [Fact]
    public void ReleaseWorkflow_ShouldRestoreTheCheckerWithTheRunsOwnToken()
    {
        var path = Path.Combine(FindRepositoryRoot(), ".github", "workflows", "release.yml");
        var stream = new YamlStream();
        using var input = File.OpenText(path);
        stream.Load(input);
        var workflow = (YamlMappingNode) stream.Documents[0].RootNode;
        var build = (YamlMappingNode) ((YamlMappingNode) workflow["jobs"])["build"];

        ((YamlMappingNode) workflow["permissions"])["packages"].ToString().Should().Be("read");
        ((YamlMappingNode) build["env"])["NuGetPackageSourceCredentials_specht"].ToString()
            .Should().Contain("Password=${{ secrets.GITHUB_TOKEN }}");
        Regex.Matches(File.ReadAllText(path), @"secrets\.(\w+)").Select(static match => match.Groups[1].Value).Distinct()
            .Should().Equal("GITHUB_TOKEN");
    }

    [Fact]
    public async Task PushReleasePackages_WhenRunOnATagPush_ShouldPushBothPackagesToNuGetOrgWithTheIssuedKey()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        using var fixture = new ReleaseFixture();
        fixture.WritePackages();
        var (ExitCode, Output) = await fixture.RunTarget("PushReleasePackages", "push");
        ExitCode.Should().Be(0, Output);
        var pushes = File.ReadAllLines(fixture.DotNetLog);
        pushes.Should().HaveCount(2).And.AllSatisfy(static push => push.Should()
            .StartWith("nuget push ")
            .And.Contain("--source https://api.nuget.org/v3/index.json")
            .And.Contain("--api-key fixture-key"));
        pushes.Should().ContainSingle(static push => push.Contains("specht.tool.1.2.3.nupkg", StringComparison.Ordinal))
            .And.ContainSingle(static push => push.Contains("Specht.CodeAnalysis.1.2.3.nupkg", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("1.2.3", false)]
    [InlineData("0.3.12-preview", true)]
    public async Task CreateGitHubRelease_ShouldCreateTheReleaseWithoutAttachingPackages(string version, bool prerelease)
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        // Given
        using var fixture = new ReleaseFixture(version);

        // When
        var (ExitCode, Output) = await fixture.RunTarget("CreateGitHubRelease", "push");

        // Then
        ExitCode.Should().Be(0, Output);
        var invocation = File.ReadAllLines(fixture.GhLog)
            .Should().ContainSingle(line => line.StartsWith($"release create v{version} ", StringComparison.Ordinal)).Which;
        invocation.Should().NotContain(".nupkg").And.Contain("--generate-notes --verify-tag");
        invocation.Contains("--prerelease", StringComparison.Ordinal).Should().Be(prerelease);
    }

    [Fact]
    public async Task CreateGitHubRelease_WhenTheReleaseIsAlreadyPublishedAtThisCommit_ShouldKeepItAndSucceed()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        // Given
        using var fixture = new ReleaseFixture();

        // When
        var (ExitCode, Output) = await fixture.RunTarget("CreateGitHubRelease", "push", Published, ReleaseFixture.Commit);

        // Then
        ExitCode.Should().Be(0, Output);
        var invocations = File.ReadAllLines(fixture.GhLog);
        invocations.Should()
            .Contain(static invocation => invocation.StartsWith("release view v1.2.3 --repo RLittlesII/specht --json ", StringComparison.Ordinal))
            .And.Contain("api repos/RLittlesII/specht/commits/tags/v1.2.3 --jq .sha")
            .And.NotContain(static invocation => invocation.StartsWith("release create", StringComparison.Ordinal));
    }

    [Theory]
    [ClassData(typeof(ReleasesNotPublishedAtThisCommit))]
    public async Task CreateGitHubRelease_WhenNoReleaseIsPublishedAtThisCommit_ShouldCreateIt(string existing, string tagged)
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        // Given
        using var fixture = new ReleaseFixture();

        // When
        var (ExitCode, Output) = await fixture.RunTarget("CreateGitHubRelease", "push", existing, tagged);

        // Then
        ExitCode.Should().Be(0, Output);
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

        // When
        var (ExitCode, Output) = await fixture.RunTarget("CreateGitHubRelease", "push", Draft, ReleaseFixture.Commit, createFails: true);

        // Then
        ExitCode.Should().NotBe(0);
        File.Exists(fixture.GhLog).Should().BeTrue(Output);
        File.ReadAllLines(fixture.GhLog).Should().Contain(static invocation => invocation.StartsWith("release create v1.2.3", StringComparison.Ordinal));
    }

    public sealed class ReleasesNotPublishedAtThisCommit : TheoryData<string, string>
    {
        public ReleasesNotPublishedAtThisCommit()
        {
            Add("", ReleaseFixture.Commit);
            Add(Published, "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            Add(Draft, ReleaseFixture.Commit);
            Add("""{"tagName":"v1.2.4","isDraft":false}""", ReleaseFixture.Commit);
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
        public ReleaseFixture(string version = "1.2.3")
        {
            _version = version;
            Directory.CreateDirectory(Path.Combine(Root, ".nuke"));
            Directory.CreateDirectory(Path.Combine(Root, "src", "tool"));
            Directory.CreateDirectory(Path.Combine(Root, "src", "codeanalysis"));
            Directory.CreateDirectory(Path.Combine(Root, "bin"));
            Directory.CreateDirectory(Path.Combine(Root, "runner"));
            Directory.CreateDirectory(Path.Combine(Root, "tools"));
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
            var dotnet = Path.Combine(Root, "tools", "dotnet");
            File.WriteAllText(dotnet, "#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$DOTNET_INVOCATIONS\"\n");
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(gh, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                File.SetUnixFileMode(dotnet, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }

        public const string Commit = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        public string Root { get; } = Path.Combine(".artifacts", "release-tests", Guid.NewGuid().ToString("N"));

        public string GhLog => Path.Combine(Root, "gh-arguments.txt");

        public string DotNetLog => Path.Combine(Root, "dotnet-arguments.txt");

        public void WritePackages()
        {
            var packages = Path.Combine(Root, ".artifacts", "nupkg");
            Directory.CreateDirectory(packages);
            File.WriteAllText(Path.Combine(packages, $"specht.tool.{_version}.nupkg"), "tool");
            File.WriteAllText(Path.Combine(packages, $"Specht.CodeAnalysis.{_version}.nupkg"), "analysis");
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
            process.StartInfo.Environment["GITHUB_REF_NAME"] = $"v{_version}";
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
            process.StartInfo.Environment["NUGET_API_KEY"] = "fixture-key";
            process.StartInfo.Environment["DOTNET_EXE"] = Path.GetFullPath(Path.Combine(Root, "tools", "dotnet"));
            process.StartInfo.Environment["DOTNET_INVOCATIONS"] = Path.GetFullPath(DotNetLog);

            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            return (process.ExitCode, await stdout + await stderr);
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);

        private readonly string _version;
    }

    private const string Published = """{"tagName":"v1.2.3","isDraft":false}""";

    private const string Draft = """{"tagName":"v1.2.3","isDraft":true}""";
}
