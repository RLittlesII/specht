using System.Diagnostics;
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

    private static async Task<(int ExitCode, string Output)> Run(
        string root, string tool, string[] arguments, params (string Name, string Value)[] environment)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(tool)
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        process.StartInfo.Environment["NUKE_TELEMETRY_OPTOUT"] = "1";
        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        foreach (var (name, value) in environment)
        {
            process.StartInfo.Environment[name] = value;
        }

        process.Start();
        var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, await output + await error);
    }

    private sealed class ReleaseRepository : IDisposable
    {
        public ReleaseRepository()
        {
            Directory.CreateDirectory(Path.Combine(Root, ".nuke"));
            Directory.CreateDirectory(Path.Combine(Root, "src/tool/schema/v0.1.0"));
            Directory.CreateDirectory(Path.Combine(Root, "src/tool/schema/v0.2.0"));
        }

        public string Root { get; } = Path.Combine(".artifacts", "schema-release-tests", Guid.NewGuid().ToString("N"));

        public string LockPath => Path.Combine(Root, SchemaVersionLock.RelativePath);

        public async Task<string> Initialize()
        {
            await File.WriteAllTextAsync(Path.Combine(Root, "version.json"), "{\"version\":\"0.1\"}", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(Root, "fixture.slnx"), "<Solution />", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(Root, ".nuke/parameters.json"), "{\"Solution\":\"fixture.slnx\"}", TestContext.Current.CancellationToken);
            (await Run(Root, "git", ["init", "--initial-branch=main"])).ExitCode.Should().Be(0);
            (await Run(Root, "git", ["add", "."])).ExitCode.Should().Be(0);
            (await Run(Root, "git", ["-c", "user.name=Synthetic", "-c", "user.email=synthetic@example.invalid",
                "-c", "core.hooksPath=/dev/null", "-c", "commit.gpgsign=false", "commit", "-m", "Synthetic fixture"])).ExitCode.Should().Be(0);
            (await Run(Root, "git", ["update-ref", "refs/remotes/origin/main", "HEAD"])).ExitCode.Should().Be(0);
            var version = await Run(Root, "dotnet", ["nbgv", "get-version", "--variable", "NuGetPackageVersion"]);
            version.ExitCode.Should().Be(0, version.Output);
            return $"v{version.Output.Trim()}";
        }

        public Task<(int ExitCode, string Output)> RunTarget(string target, string tag) =>
            Run(Root, "dotnet", [typeof(SchemaVersionLock).Assembly.Location,
                "--root", Path.GetFullPath(Root), "--target", target, "--skip", "Restore", "Build"],
                ("GITHUB_REF_TYPE", "tag"), ("GITHUB_REF_NAME", tag));

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
