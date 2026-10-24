using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>The comparison and rewrite beneath <c>specht upgrade</c> (<c>0001-F7</c> B-005, B-006, B-007).</summary>
[Trait("Tier", "Unit")]
public sealed class UpgradeWriterUnitTests
{
    [Fact]
    public async Task OneDifferingAndOneEqualPresentFile_WhenUpgraded_ShouldRewriteOnlyTheDifferingOneAndNameItRelativeToTheRoot()
    {
        // Given
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 1, 0))
            .Where(static file => file.Relative != ".specht/manifest.json").ToList();
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\":\"0.1.0\"}"));
        foreach (var file in shipped)
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        fileSystem.File.WriteAllText("repo/.specht/templates/feature.md", "# A synthetic template with local edits\n");
        fileSystem.File.SetLastWriteTimeUtc("repo/.specht/templates/feature.md", stamp);

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/templates/feature.md");
        foreach (var file in shipped)
        {
            var path = "repo/" + file.Relative;
            fileSystem.File.ReadAllBytes(path).Should().Equal(file.Bytes, file.Relative);
            var untouched = file.Relative != ".specht/templates/feature.md";
            (fileSystem.File.GetLastWriteTimeUtc(path) == stamp).Should().Be(untouched, file.Relative);
        }
    }

    [Fact]
    public async Task FilesThePinnedVersionDoesNotShip_WhenUpgraded_ShouldBeLeftWithTheirBytes()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\":\"0.1.0\"}"));
        fileSystem.AddFile("repo/.specht/templates/consumer-own.md", new MockFileData("# A synthetic consumer template\n"));
        fileSystem.AddFile("repo/.specht/schema/frontmatter/consumer-own.schema.json", new MockFileData("{}"));

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().BeEmpty();
        fileSystem.File.ReadAllText("repo/.specht/templates/consumer-own.md").Should().Be("# A synthetic consumer template\n");
        fileSystem.File.ReadAllText("repo/.specht/schema/frontmatter/consumer-own.schema.json").Should().Be("{}");
    }

    [Fact]
    public async Task ShippedFilesMissingFromTheRoot_WhenUpgraded_ShouldNotBeCreatedAndTheResultNamesNoFile()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\":\"0.1.0\"}"));

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Pinned.Should().Be(new SemanticVersion(0, 1, 0));
        result.Rewritten.Should().BeEmpty();
        fileSystem.AllFiles.Should().ContainSingle();
    }
}
