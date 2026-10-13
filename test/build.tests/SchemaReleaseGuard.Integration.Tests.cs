using System.Diagnostics;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class SchemaReleaseGuardIntegrationTests
{
    [Theory]
    [InlineData(null, "0.1.0, 0.2.0")]
    [InlineData("{\"0.1.0\":\"digest\"}", "0.2.0")]
    [InlineData("{\"0.2.0\":\"digest\"}", "0.1.0")]
    [InlineData("{\"0.1.0\":\"digest\",\"0.2.0\":\"digest\"}", null)]
    public async Task ASchemaLock_WhenAReleaseTagIsChecked_ShouldGatePackingWithoutChangingTheLock(
        string? entries, string? unlocked)
    {
        // Given
        var root = Path.Combine(".artifacts", "schema-release-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, ".nuke"));
        Directory.CreateDirectory(Path.Combine(root, "src/tool/schema/v0.1.0"));
        Directory.CreateDirectory(Path.Combine(root, "src/tool/schema/v0.2.0"));
        var lockPath = Path.Combine(root, SchemaVersionLock.RelativePath);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "version.json"), "{\"version\":\"0.1\"}", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(root, "fixture.slnx"), "<Solution />", TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(Path.Combine(root, ".nuke/parameters.json"), "{\"Solution\":\"fixture.slnx\"}", TestContext.Current.CancellationToken);
            if (entries is not null)
            {
                await File.WriteAllTextAsync(lockPath, entries, TestContext.Current.CancellationToken);
            }

            (await Run(root, "git", ["init", "--initial-branch=main"])).ExitCode.Should().Be(0);
            (await Run(root, "git", ["add", "."])).ExitCode.Should().Be(0);
            (await Run(root, "git", ["-c", "user.name=Synthetic", "-c", "user.email=synthetic@example.invalid",
                "-c", "core.hooksPath=/dev/null", "-c", "commit.gpgsign=false", "commit", "-m", "Synthetic fixture"])).ExitCode.Should().Be(0);
            (await Run(root, "git", ["update-ref", "refs/remotes/origin/main", "HEAD"])).ExitCode.Should().Be(0);
            var version = await Run(root, "dotnet", ["nbgv", "get-version", "--variable", "NuGetPackageVersion"]);
            version.ExitCode.Should().Be(0, version.Output);
            var target = unlocked is null ? "VerifyTag" : "Release";

            // When
            var result = await Run(root, "dotnet", [typeof(SchemaVersionLock).Assembly.Location,
                "--root", Path.GetFullPath(root), "--target", target, "--skip", "Restore", "Build"],
                $"v{version.Output.Trim()}");

            // Then
            if (unlocked is null)
            {
                result.ExitCode.Should().Be(0, result.Output);
            }
            else
            {
                result.ExitCode.Should().NotBe(0);
                result.Output.Should().Contain($"Shipped schema versions are not locked in {SchemaVersionLock.RelativePath}: {unlocked}.");
                result.Output.Should().NotContain("║ Pack");
                Directory.Exists(Path.Combine(root, ".artifacts/nupkg")).Should().BeFalse();
            }

            if (entries is null)
            {
                File.Exists(lockPath).Should().BeFalse();
            }
            else
            {
                (await File.ReadAllTextAsync(lockPath, TestContext.Current.CancellationToken)).Should().Be(entries);
            }
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<(int ExitCode, string Output)> Run(string root, string tool, string[] arguments, string? tag = null)
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

        if (tag is not null)
        {
            process.StartInfo.Environment["GITHUB_REF_TYPE"] = "tag";
            process.StartInfo.Environment["GITHUB_REF_NAME"] = tag;
        }

        process.Start();
        var output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, await output + await error);
    }
}
