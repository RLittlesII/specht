using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Manifest;

namespace Specht.Tests.Manifest;

/// <summary>The staged replace beneath <c>specht upgrade</c> and the one place that enforces its write set (<c>0001-F7</c> B-007, C-2).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestStoreReplaceConfigurationFileUnitTests
{
    [Theory]
    [ClassData(typeof(RefusedConfigurationPaths))]
    public async Task PathOutsideTheAllowedFolders_WhenReplacing_ShouldRefuseAndWriteNothing(string relative)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo");
        var store = new SpecManifestStore(fileSystem);

        // When
        Func<Task> action = () => store.ReplaceConfigurationFile("repo", relative, [1, 2, 3], CancellationToken.None);

        // Then
        await action.Should().ThrowAsync<SpechtManifestException>();
        fileSystem.AllFiles.Should().BeEmpty();
    }

    [Theory]
    [InlineData(".specht/templates/feature.md")]
    [InlineData(".specht/schema/frontmatter/feature.schema.json")]
    [InlineData(".specht/manifest.json")]
    public async Task PathInAnAllowedPlace_WhenReplacing_ShouldHoldTheNewBytesAndLeaveNoStagingSibling(string relative)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/" + relative, new MockFileData([9, 9]));
        var store = new SpecManifestStore(fileSystem);

        // When
        await store.ReplaceConfigurationFile("repo", relative, [1, 2, 3], CancellationToken.None);

        // Then
        fileSystem.File.ReadAllBytes("repo/" + relative).Should().Equal(1, 2, 3);
        fileSystem.AllFiles.Should().ContainSingle();
    }
}
