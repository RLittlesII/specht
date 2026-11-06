using System.Diagnostics;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Build;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// Steps for the release scenario of 0132-F3 B-035 (item 0363). The packages are the files the solution pack writes, read from
/// <see cref="PackedTool"/> and copied into a synthetic root beside this repository's own project files; the compiled build's
/// <c>PushReleasePackages</c> target then runs there as a tag-push run, with <c>DOTNET_EXE</c> pointing at a script that records its
/// arguments, so nothing is pushed. The tag is the version of the packed tool.
/// </summary>
[Binding]
[Scope(Feature = "The MCP host")]
public sealed class McpPackageSteps(PackedTool tool) : IDisposable
{
    public void Dispose()
    {
        if (_root is not null)
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Given("a repository release")]
    public async Task GivenARepositoryRelease()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The release job runs on Ubuntu.");
        _root = Directory.CreateTempSubdirectory("specht-mcp-release-").FullName;
        _packed = [await tool.PackageAsync(), await tool.CodeAnalysisPackageAsync(), await tool.McpPackageAsync()];
        Directory.CreateDirectory(Path.Combine(_root, ".nuke"));
        Directory.CreateDirectory(Path.Combine(_root, ".artifacts", "nupkg"));
        Directory.CreateDirectory(Path.Combine(_root, "runner"));
        File.WriteAllText(Path.Combine(_root, "fixture.slnx"), "<Solution />");
        File.WriteAllText(Path.Combine(_root, ".nuke", "parameters.json"), "{\"Solution\":\"fixture.slnx\"}");
        foreach (var project in new[] { "tool/tool.csproj", "codeanalysis/codeanalysis.csproj", "mcp/mcp.csproj" })
        {
            Directory.CreateDirectory(Path.Combine(_root, "src", Path.GetDirectoryName(project)!));
            File.Copy(Path.Combine(tool.Repository, "src", project), Path.Combine(_root, "src", project));
        }

        foreach (var package in _packed)
        {
            File.Copy(package, Path.Combine(_root, ".artifacts", "nupkg", Path.GetFileName(package)));
        }

        var dotnet = Path.Combine(_root, "dotnet");
        File.WriteAllText(dotnet, "#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$DOTNET_INVOCATIONS\"\n");
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(dotnet, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    [When("its packages are published")]
    public async Task WhenItsPackagesArePublished()
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = _root!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        process.StartInfo.ArgumentList.Add(typeof(SchemaVersionLock).Assembly.Location);
        process.StartInfo.ArgumentList.Add("--root");
        process.StartInfo.ArgumentList.Add(_root!);
        process.StartInfo.ArgumentList.Add("--target");
        process.StartInfo.ArgumentList.Add("PushReleasePackages");
        process.StartInfo.Environment["NUKE_TELEMETRY_OPTOUT"] = "1";
        process.StartInfo.Environment["GITHUB_ACTIONS"] = "true";
        process.StartInfo.Environment["GITHUB_EVENT_NAME"] = "push";
        process.StartInfo.Environment["GITHUB_REF_TYPE"] = "tag";
        process.StartInfo.Environment["GITHUB_REF_NAME"] = $"v{VersionOf(_packed![0], "specht.tool.")}";
        process.StartInfo.Environment["GITHUB_ENV"] = Path.Combine(_root!, "runner", "environment");
        process.StartInfo.Environment["GITHUB_OUTPUT"] = Path.Combine(_root!, "runner", "output");
        process.StartInfo.Environment["GITHUB_STEP_SUMMARY"] = Path.Combine(_root!, "runner", "summary");
        process.StartInfo.Environment["GITHUB_PATH"] = Path.Combine(_root!, "runner", "path");
        process.StartInfo.Environment["NUGET_API_KEY"] = "fixture-key";
        process.StartInfo.Environment["DOTNET_EXE"] = Path.Combine(_root!, "dotnet");
        process.StartInfo.Environment["DOTNET_INVOCATIONS"] = Path.Combine(_root!, "pushes.txt");

        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        process.ExitCode.Should().Be(0, await stdout + await stderr);
    }

    [Then("the MCP package is published to nuget.org with the tool and analysis packages")]
    public void ThenTheMcpPackageIsPublishedWithTheToolAndAnalysisPackages()
    {
        var pushes = File.ReadAllLines(Path.Combine(_root!, "pushes.txt"));
        pushes.Should().AllSatisfy(static push => push.Should().StartWith("nuget push ").And.Contain("--source https://api.nuget.org/v3/index.json"));
        pushes.Select(static push => Path.GetFileName(push.Split(' ')[2])).Should().BeEquivalentTo(_packed!.Select(static path => Path.GetFileName(path)));
    }

    [Then("the three carry the repository release version")]
    public void ThenTheThreeCarryTheRepositoryReleaseVersion()
    {
        var version = VersionOf(_packed![0], "specht.tool.");
        VersionOf(_packed[1], "Specht.CodeAnalysis.").Should().Be(version);
        VersionOf(_packed[2], "specht.mcp.").Should().Be(version);
    }

    private static string VersionOf(string path, string prefix) => Path.GetFileNameWithoutExtension(path)[prefix.Length..];

    private string? _root;
    private string[]? _packed;
}
