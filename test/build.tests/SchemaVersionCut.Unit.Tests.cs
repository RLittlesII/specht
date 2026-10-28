using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Unit")]
public sealed class SchemaVersionCutUnitTests
{
    [Fact]
    public async Task AShippedVersionTheLockDoesNotRecord_WhenCut_ShouldFailAndCreateNoFile()
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", "0.2.0" })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json",
                new MockFileData($"{{\"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
        }

        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.1.0\":\"earlier-digest\"}"));
        var before = fileSystem.AllFiles.ToArray();

        // When
        var cut = () => new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        await cut.Should().ThrowAsync<InvalidOperationException>();
        fileSystem.AllFiles.Except(before).Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(RefusedCutVersions))]
    public async Task ANamedVersionNotHigherOrNotMajorMinorPatch_WhenCut_ShouldFailAndCreateNoFile(string version)
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var shipped in new[] { "0.1.0", "0.2.0" })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{shipped}/frontmatter/feature.schema.json",
                new MockFileData($"{{\"$id\": \"https://github.com/rlittlesii/specht/schema/v{shipped}/frontmatter/feature.schema.json\"}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{shipped}/feature.md", new MockFileData("template\n"));
        }

        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.1.0\":\"earlier-digest\",\"0.2.0\":\"newest-digest\"}"));
        var before = fileSystem.AllFiles.ToArray();

        // When
        var cut = () => new SchemaVersionCut(fileSystem, "repo").CutAsync(version);

        // Then
        await cut.Should().ThrowAsync<InvalidOperationException>();
        fileSystem.AllFiles.Except(before).Should().BeEmpty();
    }

    [Fact]
    public async Task AVersionHigherOnlyByNumericOrder_WhenCut_ShouldCreateItsFolder()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(
            "repo/src/tool/schema/v0.9.0/frontmatter/feature.schema.json",
            new MockFileData("{\"$id\": \"https://github.com/rlittlesii/specht/schema/v0.9.0/frontmatter/feature.schema.json\"}\n"));
        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.9.0\":\"newest-digest\"}"));

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.10.0");

        // Then
        fileSystem.File.Exists("repo/src/tool/schema/v0.10.0/frontmatter/feature.schema.json").Should().BeTrue();
    }

    [Fact]
    public async Task TheNewestVersionInEveryTree_WhenCut_ShouldGiveEachTreeOneFileForEachOfItsFilesAndNoOther()
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", "0.2.0" })
        {
            foreach (var name in new[] { "feature", "task" })
            {
                fileSystem.AddFile(
                    $"repo/src/tool/schema/v{version}/frontmatter/{name}.schema.json",
                    new MockFileData($"{{\"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/{name}.schema.json\"}}\n"));
            }

            fileSystem.AddFile($"repo/src/tool/manifest/v{version}/manifest.json", new MockFileData($"{{\"schemaVersion\": \"{version}\"}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
            fileSystem.AddFile($"repo/docs/rules/v{version}/SPEC001.md", new MockFileData($"| Schema version   | {version}       |\n"));
        }

        fileSystem.AddFile("repo/src/tool/templates/v0.2.0/nested/deeper/lesson.md", new MockFileData("nested template\n"));
        fileSystem.AddFile("repo/docs/rules/v0.1.0/SPEC099.md", new MockFileData("| Schema version   | 0.1.0       |\n"));
        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.1.0\":\"earlier-digest\",\"0.2.0\":\"newest-digest\"}"));
        string[] Under(string folder) => fileSystem.Directory
            .GetFiles(folder, "*", SearchOption.AllDirectories)
            .Select(path => fileSystem.Path.GetRelativePath(folder, path).Replace('\\', '/'))
            .ToArray();

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        foreach (var tree in new[] { "src/tool/schema", "src/tool/manifest", "src/tool/templates", "docs/rules" })
        {
            Under($"repo/{tree}/v0.3.0").Should().BeEquivalentTo(Under($"repo/{tree}/v0.2.0"));
        }
    }

    [Fact]
    public async Task ATreeWithoutTheNewestVersion_WhenCut_ShouldGetNoFolderForTheNewVersion()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(
            "repo/src/tool/schema/v0.2.0/frontmatter/feature.schema.json",
            new MockFileData("{\"$id\": \"https://github.com/rlittlesii/specht/schema/v0.2.0/frontmatter/feature.schema.json\"}\n"));
        fileSystem.AddFile("repo/docs/rules/v0.1.0/SPEC001.md", new MockFileData("| Schema version   | 0.1.0       |\n"));
        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.2.0\":\"newest-digest\"}"));

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        fileSystem.Directory.Exists("repo/src/tool/schema/v0.3.0").Should().BeTrue();
        fileSystem.Directory.Exists("repo/docs/rules/v0.3.0").Should().BeFalse();
        fileSystem.Directory.Exists("repo/src/tool/templates/v0.3.0").Should().BeFalse();
    }

    [Fact]
    public async Task EachFrontmatterSchemaOfTheNewestVersion_WhenCut_ShouldDeclareTheAddressOfTheNewVersion()
    {
        // Given
        var fileSystem = new MockFileSystem();
        var names = new[] { "feature.schema.json", "task.schema.json", "epic.schema.json" };
        foreach (var name in names)
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v0.2.0/frontmatter/{name}",
                new MockFileData($"{{\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v0.2.0/frontmatter/{name}\",\n  \"type\": \"object\"\n}}\n"));
        }

        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.2.0\":\"newest-digest\"}"));

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        foreach (var name in names)
        {
            using var schema = JsonDocument.Parse(
                await fileSystem.File.ReadAllTextAsync($"repo/src/tool/schema/v0.3.0/frontmatter/{name}", TestContext.Current.CancellationToken));
            schema.RootElement.GetProperty("$id").GetString()
                .Should().Be($"https://github.com/rlittlesii/specht/schema/v0.3.0/frontmatter/{name}");
        }
    }

    [Fact]
    public async Task EachRulePageOfTheNewestVersion_WhenCut_ShouldGiveTheNewVersionAsItsSchemaVersion()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo/src/tool/schema/v0.2.0");
        var pages = new[] { "SPEC001.md", "SPEC002.md" };
        foreach (var page in pages)
        {
            fileSystem.AddFile(
                $"repo/docs/rules/v0.2.0/{page}",
                new MockFileData(
                    "## Metadata\n\n| Property         | Value       |\n| ---------------- | ----------- |\n| Schema version   | 0.2.0       |\n"));
        }

        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.2.0\":\"newest-digest\"}"));

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        foreach (var page in pages)
        {
            var lines = await fileSystem.File.ReadAllLinesAsync($"repo/docs/rules/v0.3.0/{page}", TestContext.Current.CancellationToken);
            lines.Where(static line => line.StartsWith("| Schema version ", StringComparison.Ordinal))
                .Select(static line => line.Split('|')[2].Trim())
                .Should().Equal("0.3.0");
        }
    }

    [Theory]
    [ClassData(typeof(CutFileBytes))]
    public async Task AFileOfTheNewestVersion_WhenCut_ShouldKeepEveryByteButTheStatedVersion(
        string source, string cut, byte[] original, byte[] expected)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("repo/src/tool/schema/v0.2.0");
        fileSystem.AddFile($"repo/{source}", new MockFileData(original));
        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.2.0\":\"newest-digest\"}"));

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        (await fileSystem.File.ReadAllBytesAsync($"repo/{cut}", TestContext.Current.CancellationToken)).Should().Equal(expected);
    }

    [Fact]
    public async Task FilesOutsideTheNewVersionsFolders_WhenCut_ShouldBeNeitherCreatedChangedNorDeleted()
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", "0.2.0" })
        {
            fileSystem.AddFile(
                $"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json",
                new MockFileData($"{{\"$id\": \"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/feature.schema.json\"}}\n"));
            fileSystem.AddFile($"repo/src/tool/manifest/v{version}/manifest.json", new MockFileData($"{{\"schemaVersion\": \"{version}\"}}\n"));
            fileSystem.AddFile($"repo/src/tool/templates/v{version}/feature.md", new MockFileData("template\n"));
            fileSystem.AddFile($"repo/docs/rules/v{version}/SPEC001.md", new MockFileData($"| Schema version   | {version}       |\n"));
        }

        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.1.0\":\"earlier-digest\",\"0.2.0\":\"newest-digest\"}"));
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData("{\"schemaVersion\": \"0.2.0\"}\n"));
        fileSystem.AddFile("repo/README.md", new MockFileData("readme\n"));
        var before = fileSystem.AllFiles.ToDictionary(static path => path, path => fileSystem.File.ReadAllBytes(path));
        var newFolders = new[] { "src/tool/schema", "src/tool/manifest", "src/tool/templates", "docs/rules" }
            .Select(tree => fileSystem.Path.GetFullPath($"repo/{tree}/v0.3.0") + fileSystem.Path.DirectorySeparatorChar)
            .ToArray();

        // When
        await new SchemaVersionCut(fileSystem, "repo").CutAsync("0.3.0");

        // Then
        fileSystem.AllFiles
            .Where(path => !newFolders.Any(folder => path.StartsWith(folder, StringComparison.Ordinal)))
            .ToDictionary(static path => path, path => fileSystem.File.ReadAllBytes(path))
            .Should().BeEquivalentTo(before);
    }
}
