using System.Diagnostics;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class RunnerCommandFilesIntegrationTests
{
    [Fact]
    public async Task RepositoryBuild_WhenRunAsARunnerStep_ShouldWriteNothingToTheRunnerEnvironmentFile()
    {
        // Given
        var root = FindRepositoryRoot();
        var scratch = Path.Combine(root, ".artifacts", "runner-command-files-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        var environment = Path.Combine(scratch, "environment");
        await File.WriteAllTextAsync(environment, string.Empty, TestContext.Current.CancellationToken);

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            process.StartInfo.ArgumentList.Add("msbuild");
            process.StartInfo.ArgumentList.Add("src/specht/specht.csproj");
            process.StartInfo.ArgumentList.Add("-target:GetBuildVersion");
            process.StartInfo.ArgumentList.Add("-property:TargetFramework=net10.0");
            process.StartInfo.ArgumentList.Add("-nodeReuse:false");
            process.StartInfo.Environment["GITHUB_ACTIONS"] = "true";
            process.StartInfo.Environment["GITHUB_ENV"] = environment;

            // When
            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            var stderr = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);

            // Then
            process.ExitCode.Should().Be(0, await stdout + await stderr);
            (await File.ReadAllTextAsync(environment, TestContext.Current.CancellationToken)).Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(scratch, recursive: true);
        }
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
