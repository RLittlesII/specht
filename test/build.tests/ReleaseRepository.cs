using System.Diagnostics;
using AwesomeAssertions;

namespace Specht.Build.Tests;

internal sealed class ReleaseRepository : IDisposable
{
    public ReleaseRepository(string version = "0.1-preview")
    {
        _version = version;
        Directory.CreateDirectory(Path.Combine(Root, ".nuke"));
        Directory.CreateDirectory(Path.Combine(Root, "src/tool/schema/v0.1.0"));
        Directory.CreateDirectory(Path.Combine(Root, "src/tool/schema/v0.2.0"));
    }

    public string Root { get; } = Path.Combine(".artifacts", "schema-release-tests", Guid.NewGuid().ToString("N"));

    public string LockPath => Path.Combine(Root, SchemaVersionLock.RelativePath);

    public async Task<string> Initialize()
    {
        await File.WriteAllTextAsync(
            Path.Combine(Root, "version.json"),
            $"{{\"version\":\"{_version}\",\"publicReleaseRefSpec\":[\"^refs/heads/main$\"]}}",
            TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(Root, "fixture.slnx"), "<Solution />", TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(Root, ".nuke/parameters.json"), "{\"Solution\":\"fixture.slnx\"}", TestContext.Current.CancellationToken);
        (await Run("git", ["init", "--initial-branch=main"])).ExitCode.Should().Be(0);
        (await Run("git", ["add", "."])).ExitCode.Should().Be(0);
        (await Run("git", ["-c", "user.name=Synthetic", "-c", "user.email=synthetic@example.invalid",
            "-c", "core.hooksPath=/dev/null", "-c", "commit.gpgsign=false", "commit", "-m", "Synthetic fixture"])).ExitCode.Should().Be(0);
        (await Run("git", ["update-ref", "refs/remotes/origin/main", "HEAD"])).ExitCode.Should().Be(0);
        var (ExitCode, Output) = await Run("dotnet", ["nbgv", "get-version", "--variable", "NuGetPackageVersion"]);
        ExitCode.Should().Be(0, Output);
        return $"v{Output.Trim()}";
    }

    public Task<(int ExitCode, string Output)> RunTarget(string target, string tag) =>
        Run("dotnet", [typeof(SchemaVersionLock).Assembly.Location,
            "--root", Path.GetFullPath(Root), "--target", target, "--skip", "Restore", "Build"],
            ("GITHUB_REF_TYPE", "tag"), ("GITHUB_REF_NAME", tag));

    public void Dispose() => Directory.Delete(Root, recursive: true);

    private async Task<(int ExitCode, string Output)> Run(
        string tool, string[] arguments, params (string Name, string Value)[] environment)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo(tool)
        {
            WorkingDirectory = Root,
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

    private readonly string _version;
}
