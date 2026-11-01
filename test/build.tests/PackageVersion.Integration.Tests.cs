using System.Diagnostics;
using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class PackageVersionIntegrationTests : IDisposable
{
    [Fact]
    public async Task APreviewVersionTag_WhenItsVersionIsComputed_ShouldNameNoCommit()
    {
        // Given
        File.Copy(Path.Combine(_repository, "version.json"), Path.Combine(_synthetic, "version.json"));
        await Run(_synthetic, [], "git", "init", "--initial-branch=topic");
        await Run(_synthetic, [], "git", "add", "version.json");
        await Run(_synthetic, [], "git", "-c", "user.name=Synthetic", "-c", "user.email=synthetic@example.invalid",
            "-c", "core.hooksPath=/dev/null", "-c", "commit.gpgsign=false", "commit", "-m", "Synthetic version line");
        var commit = (await Run(_synthetic, [], "git", "rev-parse", "HEAD")).Trim();
        var local = JsonNode.Parse(await Run(_repository, [("GITHUB_ACTIONS", string.Empty)],
            "dotnet", "nbgv", "get-version", "-p", _synthetic, "-f", "json"))!;
        var tag = $"refs/tags/v{local["SimpleVersion"]}{local["PrereleaseVersion"]}";

        // When
        var version = (await Run(_repository,
            [
                ("GITHUB_ACTIONS", "true"), ("GITHUB_EVENT_NAME", "push"), ("GITHUB_HEAD_REF", string.Empty), ("GITHUB_REF", tag),
                ("GITHUB_ENV", Path.Combine(_runner, "environment")), ("GITHUB_OUTPUT", Path.Combine(_runner, "output")),
                ("GITHUB_STEP_SUMMARY", Path.Combine(_runner, "summary")), ("GITHUB_PATH", Path.Combine(_runner, "path")),
            ],
            "dotnet", "nbgv", "get-version", "-p", _synthetic, "--variable", "NuGetPackageVersion")).Trim();

        // Then
        version.Should().NotContain(commit[..7]);
    }

    public void Dispose()
    {
        Directory.Delete(_synthetic, recursive: true);
        Directory.Delete(_runner, recursive: true);
    }

    private static async Task<string> Run(
        string directory, (string Name, string Value)[] environment, string tool, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(tool)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
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
        process.ExitCode.Should().Be(0, await error);

        return await output;
    }

    private static string FindRepositoryRoot(DirectoryInfo? directory) =>
        directory is null ? throw new DirectoryNotFoundException("Could not locate the repository root.")
        : File.Exists(Path.Combine(directory.FullName, "specht.slnx")) ? directory.FullName
        : FindRepositoryRoot(directory.Parent);

    private readonly string _repository = FindRepositoryRoot(new DirectoryInfo(AppContext.BaseDirectory));
    private readonly string _synthetic = Directory.CreateTempSubdirectory("specht-version-tag-").FullName;
    private readonly string _runner = Directory.CreateTempSubdirectory("specht-version-runner-").FullName;
}
