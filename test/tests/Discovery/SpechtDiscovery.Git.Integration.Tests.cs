using System.Diagnostics;
using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// Discovery over a <see cref="SpechtTree"/> that a real <c>git</c> has made a work tree, in a temporary directory outside
/// every other one (<c>0001-F6</c> B-005, B-010; item 0008). The child process is the production one, wrapped so a test can
/// read what each git listing returned.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtDiscoveryGitIntegrationTests
{
    [Fact]
    public void TrackedUntrackedAndIgnoredSpecifications_WhenDiscoveredInAWorkTree_ShouldFindTheTrackedAndUntrackedAndNoneUnderAnIgnoredDirectory()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(".gitignore", "dist/\n");
        tree.WriteRaw("src/tracked/.spec/README.md", string.Empty);
        tree.WriteRaw("src/untracked/.spec/README.md", string.Empty);
        tree.WriteRaw("dist/one/two/three/.spec/README.md", string.Empty);
        Git(tree.Root, "init", "--quiet");
        Git(tree.Root, "add", ".gitignore", "src/tracked/.spec/README.md");
        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;

        // When
        var discovered = new SpechtDiscovery(fileSystem, new ChildProcess()).FindSpecifications(tree.Root, inputs)
            .Select(static location => location.RelativePath);

        // Then
        discovered.Should().Equal("src/tracked/.spec/README.md", "src/untracked/.spec/README.md");
    }

    [Fact]
    public void AManifestsExclusions_WhenGitListsAWorkTree_ShouldListNoFileUnderAnExcludedDirectoryTrackedOrNot()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw("src/area/.spec/README.md", string.Empty);
        tree.WriteRaw(".spec/README.md", string.Empty);
        tree.WriteRaw("src/bin/one/.spec/README.md", string.Empty);
        tree.WriteRaw("tools/bin/two/.spec/README.md", string.Empty);
        Git(tree.Root, "init", "--quiet");
        Git(tree.Root, "add", "src/area/.spec/README.md", ".spec/README.md", "src/bin/one/.spec/README.md");
        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;
        var git = new RecordingChildProcess();

        // When
        var discovered = new SpechtDiscovery(fileSystem, git).FindSpecifications(tree.Root, inputs).Select(static location => location.RelativePath);

        // Then
        inputs.Exclusions.Should().Contain(["/.spec", "bin"]);
        var listed = git.Results.Should().ContainSingle().Which!.StandardOutput.Split('\0', StringSplitOptions.RemoveEmptyEntries);
        listed.Should().Contain("src/area/.spec/README.md");
        listed.Should().NotContain(static path => path.StartsWith(".spec/", StringComparison.Ordinal) || path.Split('/').Contains("bin"));
        discovered.Should().Equal("src/area/.spec/README.md");
    }

    private static void Git(string root, params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        process.ExitCode.Should().Be(0, output + error.GetAwaiter().GetResult());
    }

    private sealed class RecordingChildProcess : IChildProcess
    {
        public List<ChildProcessResult?> Results { get; } = [];

        public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory)
        {
            var result = new ChildProcess().Run(fileName, arguments, workingDirectory);
            Results.Add(result);
            return result;
        }
    }
}
