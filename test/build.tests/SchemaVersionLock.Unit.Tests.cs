using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Unit")]
public sealed class SchemaVersionLockUnitTests
{
    [Fact]
    public async Task AnUnlockedNewestVersion_WhenLocked_ShouldAddOnlyItsDigestAndKeepExistingEntries()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.9.0/frontmatter/feature.schema.json", new MockFileData("older"));
        fileSystem.AddFile("repo/src/tool/schema/v0.10.0/frontmatter/feature.schema.json", new MockFileData("newer"));
        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{\"0.9.0\":\"old-digest\"}"));
        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        var expected = await versionLock.ComputeDigestAsync("0.10.0");

        // When
        await versionLock.LockNewestAsync();

        // Then
        var entries = JsonSerializer.Deserialize<Dictionary<string, string>>(
            await fileSystem.File.ReadAllTextAsync($"repo/{SchemaVersionLock.RelativePath}", TestContext.Current.CancellationToken))!;
        entries.Should().BeEquivalentTo(new Dictionary<string, string> { ["0.9.0"] = "old-digest", ["0.10.0"] = expected });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AnAlreadyLockedVersion_WhenLockedAgain_ShouldKeepTheLocksBytes(bool changed)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.1.0/frontmatter/feature.schema.json", new MockFileData("schema"));
        fileSystem.AddFile("repo/src/tool/templates/v0.1.0/feature.md", new MockFileData("template"));
        const string original = "{ \"0.1.0\" : \"original-digest\" }\n";
        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData(original));
        if (changed)
        {
            fileSystem.AddFile("repo/src/tool/templates/v0.1.0/feature.md", new MockFileData("changed template"));
        }

        // When
        await new SchemaVersionLock(fileSystem, "repo").LockNewestAsync();

        // Then
        var after = await fileSystem.File.ReadAllBytesAsync(
            $"repo/{SchemaVersionLock.RelativePath}", TestContext.Current.CancellationToken);
        after.Should().Equal(Encoding.UTF8.GetBytes(original));
    }

    [Fact]
    public async Task AVersionAcrossEveryShippingTree_WhenHashed_ShouldHashOrdinalPathsAndExactBytes()
    {
        // Given
        var fileSystem = new MockFileSystem();
        var files = new Dictionary<string, byte[]>
        {
            ["src/tool/templates/v0.1.0/feature.md"] = [0, 255, 13, 10],
            ["src/tool/schema/v0.1.0/frontmatter/feature.schema.json"] = Encoding.UTF8.GetBytes("schema\r\n"),
            ["src/tool/manifest/v0.1.0/manifest.json"] = Encoding.UTF8.GetBytes("manifest\n"),
            ["docs/rules/v0.1.0/SPEC001.md"] = Encoding.UTF8.GetBytes("rule\n"),
        };
        foreach (var (path, bytes) in files)
        {
            fileSystem.AddFile($"repo/{path}", new MockFileData(bytes));
        }

        fileSystem.AddFile("repo/src/tool/schema/v0.2.0/other.json", new MockFileData("another version"));
        fileSystem.AddFile($"repo/{SchemaVersionLock.RelativePath}", new MockFileData("{}"));
        const string expected = "c5ff50e38a775f786cda01182470104dd2b145b14d9d9bc918d9a2f2edd91303";

        // When
        var digest = await new SchemaVersionLock(fileSystem, "repo").ComputeDigestAsync("0.1.0");

        // Then
        digest.Should().Be(expected);
    }

    [Theory]
    [InlineData("src/tool/schema/v0.1.0/frontmatter/feature.schema.json")]
    [InlineData("src/tool/manifest/v0.1.0/manifest.json")]
    [InlineData("src/tool/templates/v0.1.0/feature.md")]
    [InlineData("docs/rules/v0.1.0/SPEC001.md")]
    public async Task AChangedVersionFile_WhenHashed_ShouldChangeTheDigest(string path)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile($"repo/{path}", new MockFileData("original"));
        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        var before = await versionLock.ComputeDigestAsync("0.1.0");
        fileSystem.AddFile($"repo/{path}", new MockFileData("changed"));

        // When
        var after = await versionLock.ComputeDigestAsync("0.1.0");

        // Then
        after.Should().NotBe(before);
    }

    [Theory]
    [InlineData("rename")]
    [InlineData("add")]
    [InlineData("delete")]
    public async Task AChangedVersionFileSet_WhenHashed_ShouldChangeTheDigest(string change)
    {
        // Given
        var fileSystem = new MockFileSystem();
        const string path = "repo/src/tool/templates/v0.1.0/feature.md";
        fileSystem.AddFile(path, new MockFileData("template"));
        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        var before = await versionLock.ComputeDigestAsync("0.1.0");
        if (change is "rename" or "delete")
        {
            fileSystem.File.Delete(path);
        }

        if (change is "rename" or "add")
        {
            fileSystem.AddFile("repo/src/tool/templates/v0.1.0/renamed.md", new MockFileData("template"));
        }

        // When
        var after = await versionLock.ComputeDigestAsync("0.1.0");

        // Then
        after.Should().NotBe(before);
    }
}
