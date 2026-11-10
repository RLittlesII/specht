using System.IO.Abstractions;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Tests.Shared;

namespace Specht.Tests.Discovery;

/// <summary>
/// Neither discovery mode discovers a file through a symbolic link to a directory under the root, whether the link leads
/// outside the root or to a directory inside it (<c>0001-F6</c> B-011, § 5 #1; item 0009). The tree is a
/// <see cref="SpechtTree"/> one folder below its temporary directory, the directory outside it beside the root, and each
/// link is made with a relative target, so the fixture holds no absolute path.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtDiscoveryLinksIntegrationTests
{
    [Theory]
    [ClassData(typeof(LinkedDirectoryModes))]
    public void ALinkToADirectoryOutsideOrInsideTheRoot_WhenSpecificationsAreDiscovered_ShouldDiscoverNoFileThroughIt(
        string because,
        bool workTree,
        bool committed)
    {
        // Given
        using var tree = new SpechtTree(["repo"]);
        tree.WriteRaw("src/a/.spec/README.md", string.Empty);
        var outside = Directory.CreateDirectory(Path.Combine(tree.Root, "..", "outside", "x", ".spec"));
        File.WriteAllText(Path.Combine(outside.FullName, "README.md"), string.Empty);
        Directory.CreateSymbolicLink(Path.Combine(tree.Root, "linked"), "../outside");
        Directory.CreateSymbolicLink(Path.Combine(tree.Root, "src", "alias"), "a");
        if (workTree)
        {
            LocalGit.Run(tree.Root, "init", "--quiet");
        }

        if (committed)
        {
            LocalGit.Run(tree.Root, "add", "--all");
            LocalGit.Run(
                tree.Root,
                "-c", "user.name=Discovery fixture",
                "-c", "user.email=discovery@example.invalid",
                "-c", "commit.gpgSign=false",
                "-c", "core.hooksPath=no-hooks",
                "commit", "--quiet", "--message", "Links");
        }

        var fileSystem = new FileSystem();
        var inputs = EngineServices.Manifest(fileSystem, tree.Root).Discovery;
        var git = new RecordingChildProcess();

        // When
        var discovered = new SpechtDiscovery(fileSystem, workTree ? git : new UnexpectedChildProcess())
            .FindSpecifications(tree.Root, inputs)
            .Select(static location => location.RelativePath);

        // Then
        File.Exists(Path.Combine(tree.Root, "linked", "x", ".spec", "README.md")).Should().BeTrue(because);
        File.Exists(Path.Combine(tree.Root, "src", "alias", ".spec", "README.md")).Should().BeTrue(because);
        git.Results.Should().HaveCount(workTree ? 1 : 0, because).And.OnlyContain(static result => result != null && result.ExitCode == 0, because);
        discovered.Should().Equal(["src/a/.spec/README.md"], because);
    }
}
