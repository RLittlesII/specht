using System.IO.Abstractions;
using System.Text;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class SchemaVersionCutIntegrationTests
{
    [Fact]
    public async Task AShippingTreeOnDisk_WhenTheNextVersionIsCut_ShouldWriteOnlyTheNewFoldersAndLeaveNoStaging()
    {
        // Given
        var root = Path.Combine(".artifacts", "schema-cut-tests", Guid.NewGuid().ToString("N"));
        byte[] bom = [0xEF, 0xBB, 0xBF];
        var shipped = new Dictionary<string, byte[]>
        {
            ["src/tool/schema/v0.2.0/frontmatter/feature.schema.json"] = [.. bom, .. Encoding.UTF8.GetBytes(
                "{\r\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v0.2.0/frontmatter/feature.schema.json\"\r\n}\r\n")],
            ["src/tool/manifest/v0.2.0/manifest.json"] = Encoding.UTF8.GetBytes("{\n  \"schemaVersion\": \"0.2.0\"\n}\n"),
            ["src/tool/templates/v0.2.0/feature.md"] = Encoding.UTF8.GetBytes("synthetic template\n"),
            ["docs/rules/v0.2.0/SPEC001.md"] = Encoding.UTF8.GetBytes(
                "| Property         | Value       |\n| ---------------- | ----------- |\n| Schema version   | 0.2.0       |\n"),
            [SchemaVersionLock.RelativePath] = Encoding.UTF8.GetBytes("{\"0.2.0\":\"newest-digest\"}"),
        };
        foreach (var (path, bytes) in shipped)
        {
            var fullPath = Path.Combine(root, path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await File.WriteAllBytesAsync(fullPath, bytes, TestContext.Current.CancellationToken);
        }

        var expected = new Dictionary<string, byte[]>(shipped)
        {
            ["src/tool/schema/v0.3.0/frontmatter/feature.schema.json"] = [.. bom, .. Encoding.UTF8.GetBytes(
                "{\r\n  \"$id\": \"https://github.com/rlittlesii/specht/schema/v0.3.0/frontmatter/feature.schema.json\"\r\n}\r\n")],
            ["src/tool/manifest/v0.3.0/manifest.json"] = Encoding.UTF8.GetBytes("{\n  \"schemaVersion\": \"0.2.0\"\n}\n"),
            ["src/tool/templates/v0.3.0/feature.md"] = Encoding.UTF8.GetBytes("synthetic template\n"),
            ["docs/rules/v0.3.0/SPEC001.md"] = Encoding.UTF8.GetBytes(
                "| Property         | Value       |\n| ---------------- | ----------- |\n| Schema version   | 0.3.0       |\n"),
        };
        var directoriesBefore = Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .ToArray();
        try
        {
            // When
            await new SchemaVersionCut(new FileSystem(), root).CutAsync("0.3.0");

            // Then
            Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .ToDictionary(path => Path.GetRelativePath(root, path).Replace('\\', '/'), File.ReadAllBytes)
                .Should().BeEquivalentTo(expected);
            Directory.GetDirectories(root, "*", SearchOption.AllDirectories)
                .Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
                .Should().BeEquivalentTo(directoriesBefore.Concat(
                [
                    "src/tool/schema/v0.3.0", "src/tool/schema/v0.3.0/frontmatter", "src/tool/manifest/v0.3.0",
                    "src/tool/templates/v0.3.0", "docs/rules/v0.3.0",
                ]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
