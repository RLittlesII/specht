using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// Discovery over a <see cref="SpechtTree"/> that a real <c>git</c> has made a work tree, in a temporary directory outside
/// every other one (<c>0001-F6</c> B-005, B-010; item 0008); an epic file in it is found through git as the walk finds it,
/// and a test-file glob with a leading directory, or with a first character a glob pathspec reads as special, lists its
/// tracked file and nothing under an excluded one (<c>0001-F6</c> B-007, B-010; item 0009). The child process is the
/// production one, wrapped so a test can read what each git listing returned.
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
        LocalGit.Run(tree.Root, "init", "--quiet");
        LocalGit.Run(tree.Root, "add", ".gitignore", "src/tracked/.spec/README.md");
        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;

        // When
        var discovered = new SpechtDiscovery(fileSystem, new ChildProcess()).FindSpecifications(tree.Root, inputs)
            .Select(static location => location.RelativePath);

        // Then
        discovered.Should().Equal("src/tracked/.spec/README.md", "src/untracked/.spec/README.md");
    }

    [Fact]
    public void AnEpicFileTrackedInAWorkTree_WhenEpicsAreDiscoveredThroughGitAndByWalking_ShouldBeFoundByBoth()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw("epics/0001-example/epic.md", string.Empty);
        LocalGit.Run(tree.Root, "init", "--quiet");
        LocalGit.Run(tree.Root, "add", "epics/0001-example/epic.md");
        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;
        var git = new RecordingChildProcess();

        // When
        var throughGit = new SpechtDiscovery(fileSystem, git).FindEpics(tree.Root, inputs);
        var walked = new SpechtDiscovery(fileSystem, new AbsentGit()).FindEpics(tree.Root, inputs);

        // Then
        git.Results.Should().ContainSingle().Which!.ExitCode.Should().Be(0);
        walked.Should().Equal("epics/0001-example/epic.md");
        throughGit.Should().Equal(walked);
    }

    [Fact]
    public void ATestFileGlobBesideAnExcludedDirectory_WhenTestFilesAreListedThroughGit_ShouldListNoFileUnderItTrackedOrNot()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw("test/u/a.cs", string.Empty);
        tree.WriteRaw("test/obj/b.cs", string.Empty);
        tree.WriteRaw("test/obj/c.cs", string.Empty);
        LocalGit.Run(tree.Root, "init", "--quiet");
        LocalGit.Run(tree.Root, "add", "test/u/a.cs", "test/obj/b.cs");
        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;
        var git = new RecordingChildProcess();

        // When
        var discovered = new SpechtDiscovery(fileSystem, git).FindTestFiles(tree.Root, ["test/**/*.cs"], inputs.Exclusions);

        // Then
        inputs.Exclusions.Should().Contain("obj");
        git.Results.Should().ContainSingle().Which!.ExitCode.Should().Be(0);
        discovered.Should().Equal("test/u/a.cs");
    }

    [Theory]
    [ClassData(typeof(GlobHeads))]
    public void AGlobWhoseFirstCharacterIsSpecialToGit_WhenTestFilesAreListedThroughGit_ShouldListItsTrackedFileAndNoneUnderAnExcludedDirectory(
        string because,
        string directory)
    {
        // Given
        if (directory.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Assert.Skip("This file system cannot name the directory " + directory + ".");
        }

        using var tree = new SpechtTree();
        tree.WriteRaw($"{directory}/a.md", string.Empty);
        tree.WriteRaw($"{directory}/obj/b.md", string.Empty);
        tree.WriteRaw($"{directory}/obj/c.md", string.Empty);
        LocalGit.Run(tree.Root, "init", "--quiet");
        LocalGit.Run(tree.Root, "--literal-pathspecs", "add", $"{directory}/a.md", $"{directory}/obj/b.md");
        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;
        var git = new RecordingChildProcess();

        // When
        var discovered = new SpechtDiscovery(fileSystem, git).FindTestFiles(tree.Root, [$"{directory}/**/*.md"], inputs.Exclusions);

        // Then
        inputs.Exclusions.Should().Contain(["/.spec", "obj"], because);
        git.Results.Should().ContainSingle(because).Which!.ExitCode.Should().Be(0, because);
        discovered.Should().Equal([$"{directory}/a.md"], because);
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
        LocalGit.Run(tree.Root, "init", "--quiet");
        LocalGit.Run(tree.Root, "add", "src/area/.spec/README.md", ".spec/README.md", "src/bin/one/.spec/README.md");
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

    private sealed class AbsentGit : IChildProcess
    {
        public ChildProcessResult? Run(string fileName, IReadOnlyList<string> arguments, string workingDirectory) => null;
    }
}
