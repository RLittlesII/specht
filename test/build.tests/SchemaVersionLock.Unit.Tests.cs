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

    [Fact]
    public async Task ALockedVersionWhoseFilesAreUnchanged_WhenVerified_ShouldReportNoFinding()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.1.0/frontmatter/feature.schema.json", new MockFileData("schema"));
        fileSystem.AddFile("repo/src/tool/manifest/v0.1.0/manifest.json", new MockFileData("manifest"));
        fileSystem.AddFile("repo/src/tool/templates/v0.1.0/feature.md", new MockFileData("template"));
        fileSystem.AddFile("repo/docs/rules/v0.1.0/SPEC001.md", new MockFileData("rule page"));
        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}",
            new MockFileData(JsonSerializer.Serialize(new Dictionary<string, string> { ["0.1.0"] = await versionLock.ComputeDigestAsync("0.1.0") })));

        // When
        var findings = await versionLock.VerifyAsync();

        // Then
        findings.Should().BeEmpty();
    }

    [Theory]
    [InlineData("src/tool/schema/v0.1.0/frontmatter/feature.schema.json")]
    [InlineData("src/tool/manifest/v0.1.0/manifest.json")]
    [InlineData("src/tool/templates/v0.1.0/feature.md")]
    [InlineData("docs/rules/v0.1.0/SPEC001.md")]
    public async Task ALockedVersionWithAnEditedFile_WhenVerified_ShouldReportThatItsDigestDoesNotMatch(string path)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.1.0/frontmatter/feature.schema.json", new MockFileData("schema"));
        fileSystem.AddFile("repo/src/tool/manifest/v0.1.0/manifest.json", new MockFileData("manifest"));
        fileSystem.AddFile("repo/src/tool/templates/v0.1.0/feature.md", new MockFileData("template"));
        fileSystem.AddFile("repo/docs/rules/v0.1.0/SPEC001.md", new MockFileData("rule page"));
        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}",
            new MockFileData(JsonSerializer.Serialize(new Dictionary<string, string> { ["0.1.0"] = await versionLock.ComputeDigestAsync("0.1.0") })));
        fileSystem.AddFile($"repo/{path}", new MockFileData("edited"));

        // When
        var findings = await versionLock.VerifyAsync();

        // Then
        findings.Should().Equal(new SchemaVersionLockFinding("0.1.0", SchemaVersionLockFault.DigestMismatch));
    }

    [Fact]
    public async Task TwoShippedVersionsTheLockDoesNotRecord_WhenVerified_ShouldReportEachAsOneOfMoreThanOneUnlocked()
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { "0.1.0", "0.2.0", "0.3.0" })
        {
            fileSystem.AddFile($"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json", new MockFileData(version));
        }

        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}",
            new MockFileData(JsonSerializer.Serialize(new Dictionary<string, string> { ["0.1.0"] = await versionLock.ComputeDigestAsync("0.1.0") })));

        // When
        var findings = await versionLock.VerifyAsync();

        // Then
        findings.Where(static finding => finding.Fault == SchemaVersionLockFault.MoreThanOneUnlocked).Should().BeEquivalentTo(
        [
            new SchemaVersionLockFinding("0.2.0", SchemaVersionLockFault.MoreThanOneUnlocked),
            new SchemaVersionLockFinding("0.3.0", SchemaVersionLockFault.MoreThanOneUnlocked),
        ]);
    }

    [Theory]
    [InlineData("0.10.0", "0.9.0", true)]
    [InlineData("0.9.0", "0.10.0", false)]
    public async Task OneShippedVersionTheLockDoesNotRecord_WhenVerified_ShouldBeReportedOnlyWhenAHigherVersionShips(
        string locked, string unlocked, bool reported)
    {
        // Given
        var fileSystem = new MockFileSystem();
        foreach (var version in new[] { locked, unlocked })
        {
            fileSystem.AddFile($"repo/src/tool/schema/v{version}/frontmatter/feature.schema.json", new MockFileData(version));
        }

        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}",
            new MockFileData(JsonSerializer.Serialize(new Dictionary<string, string> { [locked] = await versionLock.ComputeDigestAsync(locked) })));

        // When
        var findings = await versionLock.VerifyAsync();

        // Then
        findings.Should().Equal(reported ? [new SchemaVersionLockFinding(unlocked, SchemaVersionLockFault.UnlockedNotNewest)] : []);
    }

    [Theory]
    [InlineData("0.1.0", false)]
    [InlineData("0.99.9", false)]
    [InlineData("1.0.0", true)]
    [InlineData("1.4.2", true)]
    public async Task ALockedVersionWithNoFolder_WhenVerified_ShouldBeReportedOnlyFromOneZeroZeroOn(string removed, bool reported)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v2.0.0/frontmatter/feature.schema.json", new MockFileData("schema"));
        var versionLock = new SchemaVersionLock(fileSystem, "repo");
        fileSystem.AddFile(
            $"repo/{SchemaVersionLock.RelativePath}",
            new MockFileData(JsonSerializer.Serialize(new Dictionary<string, string>
            {
                [removed] = "digest-recorded-when-it-shipped",
                ["2.0.0"] = await versionLock.ComputeDigestAsync("2.0.0"),
            })));

        // When
        var findings = await versionLock.VerifyAsync();

        // Then
        findings.Should().Equal(reported ? [new SchemaVersionLockFinding(removed, SchemaVersionLockFault.LockedVersionNotShipped)] : []);
    }
}
