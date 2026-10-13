using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Unit")]
public sealed class SchemaReleaseGuardUnitTests
{
    [Fact]
    public async Task AMissingLock_WhenPublicationIsChecked_ShouldRefuseWithoutCreatingTheLock()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.1.0/schema.json", new MockFileData("older"));
        fileSystem.AddFile("repo/src/tool/schema/v0.2.0/schema.json", new MockFileData("newest"));
        var files = fileSystem.AllFiles.ToArray();
        var guard = new SchemaVersionLock(fileSystem, "repo");

        // When
        var check = () => guard.RequireAllLockedAsync();

        // Then
        await check.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Shipped schema versions are not locked in {SchemaVersionLock.RelativePath}: 0.1.0, 0.2.0.");
        fileSystem.File.Exists($"repo/{SchemaVersionLock.RelativePath}").Should().BeFalse();
        fileSystem.AllFiles.Should().BeEquivalentTo(files);
    }

    [Theory]
    [InlineData("{}", "0.1.0, 0.2.0")]
    [InlineData("{\"0.1.0\":\"digest\"}", "0.2.0")]
    [InlineData("{\"0.2.0\":\"digest\"}", "0.1.0")]
    public async Task AnUnlockedShippedVersion_WhenPublicationIsChecked_ShouldRefuseAndLeaveTheTreeUnchanged(
        string entries, string unlocked)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.1.0/schema.json", new MockFileData("older"));
        fileSystem.AddFile("repo/src/tool/schema/v0.2.0/schema.json", new MockFileData("newest"));
        var lockPath = $"repo/{SchemaVersionLock.RelativePath}";
        fileSystem.AddFile(lockPath, new MockFileData(entries));

        var files = fileSystem.AllFiles.ToArray();
        var guard = new SchemaVersionLock(fileSystem, "repo");

        // When
        var check = () => guard.RequireAllLockedAsync();

        // Then
        await check.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Shipped schema versions are not locked in {SchemaVersionLock.RelativePath}: {unlocked}.");
        fileSystem.AllFiles.Should().BeEquivalentTo(files);
        fileSystem.File.ReadAllText(lockPath).Should().Be(entries);
    }

    [Fact]
    public async Task AllShippedVersionsLocked_WhenPublicationIsChecked_ShouldPassWithoutComparingDigestsOrWriting()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("repo/src/tool/schema/v0.1.0/schema.json", new MockFileData("changed since locking"));
        fileSystem.AddFile("repo/src/tool/schema/v0.2.0/schema.json", new MockFileData("newest"));
        const string entries = "{ \"0.1.0\": \"recorded-digest\", \"0.2.0\": \"other-digest\", \"9.0.0\": \"unshipped\" }\n";
        var lockPath = $"repo/{SchemaVersionLock.RelativePath}";
        fileSystem.AddFile(lockPath, new MockFileData(entries));
        var files = fileSystem.AllFiles.ToArray();

        // When
        await new SchemaVersionLock(fileSystem, "repo").RequireAllLockedAsync();

        // Then
        fileSystem.File.ReadAllText(lockPath).Should().Be(entries);
        fileSystem.AllFiles.Should().BeEquivalentTo(files);
    }
}
