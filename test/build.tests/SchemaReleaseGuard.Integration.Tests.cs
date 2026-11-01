using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class SchemaReleaseGuardIntegrationTests
{
    [Fact]
    public async Task AMissingLock_WhenAReleaseTagIsChecked_ShouldFailBeforePackWithoutCreatingTheLock()
    {
        // Given
        using var repository = new ReleaseRepository();
        var tag = await repository.Initialize();

        // When
        var result = await repository.RunTarget("Release", tag);

        // Then
        result.ExitCode.Should().NotBe(0);
        result.Output.Should().Contain($"Shipped schema versions are not locked in {SchemaVersionLock.RelativePath}: 0.1.0, 0.2.0.");
        result.Output.Should().NotContain("║ Pack");
        Directory.Exists(Path.Combine(repository.Root, ".artifacts/nupkg")).Should().BeFalse();
        File.Exists(repository.LockPath).Should().BeFalse();
    }

    [Theory]
    [InlineData("{\"0.1.0\":\"digest\"}", "0.2.0")]
    [InlineData("{\"0.2.0\":\"digest\"}", "0.1.0")]
    public async Task AnUnlockedShippedVersion_WhenAReleaseTagIsChecked_ShouldFailBeforePackAndLeaveTheLockUnchanged(
        string entries, string unlocked)
    {
        // Given
        using var repository = new ReleaseRepository();
        await File.WriteAllTextAsync(repository.LockPath, entries, TestContext.Current.CancellationToken);
        var tag = await repository.Initialize();

        // When
        var result = await repository.RunTarget("Release", tag);

        // Then
        result.ExitCode.Should().NotBe(0);
        result.Output.Should().Contain($"Shipped schema versions are not locked in {SchemaVersionLock.RelativePath}: {unlocked}.");
        result.Output.Should().NotContain("║ Pack");
        Directory.Exists(Path.Combine(repository.Root, ".artifacts/nupkg")).Should().BeFalse();
        (await File.ReadAllTextAsync(repository.LockPath, TestContext.Current.CancellationToken)).Should().Be(entries);
    }

    [Fact]
    public async Task AllShippedVersionsLocked_WhenAReleaseTagIsChecked_ShouldPassWithoutChangingTheLock()
    {
        // Given
        using var repository = new ReleaseRepository();
        const string entries = "{\"0.1.0\":\"digest\",\"0.2.0\":\"digest\"}";
        await File.WriteAllTextAsync(repository.LockPath, entries, TestContext.Current.CancellationToken);
        var tag = await repository.Initialize();

        // When
        var result = await repository.RunTarget("VerifyTag", tag);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        (await File.ReadAllTextAsync(repository.LockPath, TestContext.Current.CancellationToken)).Should().Be(entries);
    }
}
