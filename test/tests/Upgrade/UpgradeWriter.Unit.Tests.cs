using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>The comparison and rewrite beneath <c>specht upgrade</c> (<c>0001-F7</c> B-005, B-006, B-007, B-029; C-2).</summary>
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

    [Theory]
    [ClassData(typeof(UpgradeRootSources))]
    public async Task HandEditedSchemasUnderADiskOrUpstreamSource_WhenUpgraded_ShouldLeaveThemByteIdenticalAndStillRewriteTheTemplates(string manifest)
    {
        // Given
        var edited = Encoding.UTF8.GetBytes("{\"title\":\"A synthetic consumer schema\"}\r\n");
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0))
            .Where(static file => file.Relative != ".specht/manifest.json").ToList();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest));
        foreach (var file in shipped)
        {
            var isSchema = file.Relative.StartsWith(".specht/schema/frontmatter/", StringComparison.Ordinal);
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(isSchema ? edited : file.Bytes));
        }

        fileSystem.File.WriteAllText("repo/.specht/templates/feature.md", "# A synthetic template with local edits\n");

        // When
        var result = await UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/templates/feature.md");
        foreach (var file in shipped)
        {
            var bytes = fileSystem.File.ReadAllBytes("repo/" + file.Relative);
            if (file.Relative.StartsWith(".specht/schema/frontmatter/", StringComparison.Ordinal))
            {
                bytes.Should().Equal(edited, file.Relative);
            }
            else
            {
                bytes.Should().Equal(file.Bytes, file.Relative);
            }
        }
    }

    [Fact]
    public async Task ATemplateThatCannotBeReplaced_WhenUpgraded_ShouldThrowNamingTheRootRelativeFileAndLeaveItsBytes()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\":\"0.1.0\"}"));
        fileSystem.AddFile(
            "repo/.specht/templates/feature.md",
            new MockFileData("# A synthetic template with local edits\n") { Attributes = FileAttributes.ReadOnly });

        // When
        var upgrade = () => UpgradeServices.Writer(fileSystem).Upgrade("repo", CancellationToken.None);

        // Then
        (await upgrade.Should().ThrowAsync<SpechtManifestException>())
            .WithMessage(".specht/templates/feature.md: could not be replaced.");
        fileSystem.File.ReadAllText("repo/.specht/templates/feature.md").Should().Be("# A synthetic template with local edits\n");
    }
}
