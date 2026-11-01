using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class PrereleaseGuardIntegrationTests
{
    [Fact]
    public async Task AVersionBelowOneWithoutALabel_WhenAReleaseTagIsChecked_ShouldFailBeforePack()
    {
        // Given
        using var repository = new ReleaseRepository("0.1");
        await File.WriteAllTextAsync(repository.LockPath, LockedEntries, TestContext.Current.CancellationToken);
        var tag = await repository.Initialize();

        // When
        var result = await repository.RunTarget("Release", tag);

        // Then
        result.ExitCode.Should().NotBe(0);
        result.Output.Should().Contain($"The version '{tag[1..]}' is below 1.0.0 and carries no prerelease label");
        result.Output.Should().NotContain("║ Pack");
    }

    [Theory]
    [InlineData("0.1-preview")]
    [InlineData("1.0")]
    public async Task ALabelledOrStableVersion_WhenAReleaseTagIsChecked_ShouldPass(string version)
    {
        // Given
        using var repository = new ReleaseRepository(version);
        await File.WriteAllTextAsync(repository.LockPath, LockedEntries, TestContext.Current.CancellationToken);
        var tag = await repository.Initialize();

        // When
        var result = await repository.RunTarget("VerifyTag", tag);

        // Then
        result.ExitCode.Should().Be(0, result.Output);
    }

    private const string LockedEntries = "{\"0.1.0\":\"digest\",\"0.2.0\":\"digest\"}";
}
