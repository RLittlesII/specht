using System.Diagnostics;
using AwesomeAssertions;
using YamlDotNet.RepresentationModel;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class CodeAnalysisReleaseIntegrationTests
{
    [Fact]
    public async Task ReleaseWorkflow_WhenPackagesArePresent_ShouldPushAndAttachBothAndFailClosedIfOneIsMissing()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("The release job runs on Ubuntu.");
        }

        var scripts = LoadReleaseScripts();
        var directory = Directory.CreateTempSubdirectory("specht-release-");

        try
        {
            var packages = Path.Combine(directory.FullName, ".artifacts", "nupkg");
            var bin = Path.Combine(directory.FullName, "bin");
            var log = Path.Combine(directory.FullName, "invocations.txt");
            Directory.CreateDirectory(packages);
            Directory.CreateDirectory(bin);
            var tool = Path.Combine(packages, "specht.tool.1.2.3.nupkg");
            var analysis = Path.Combine(packages, "Specht.CodeAnalysis.1.2.3.nupkg");
            File.WriteAllText(tool, "tool");
            File.WriteAllText(analysis, "analysis");
            foreach (var command in new[] { "dotnet", "gh" })
            {
                var path = Path.Combine(bin, command);
                File.WriteAllText(path, "#!/bin/sh\nprintf '%s\\n' \"$*\" >> \"$INVOCATIONS\"\n");
                if (!OperatingSystem.IsWindows())
                {
                    File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
                }
            }

            var push = await Run(scripts.Push, directory.FullName, bin, log);
            push.ExitCode.Should().Be(0, push.Output);
            var pushed = File.ReadAllLines(log);
            pushed.Should().HaveCount(2);
            pushed[0].Should().Contain("specht.tool.1.2.3.nupkg");
            pushed[1].Should().Contain("Specht.CodeAnalysis.1.2.3.nupkg");

            var release = await Run(scripts.Release, directory.FullName, bin, log);
            release.ExitCode.Should().Be(0, release.Output);
            File.ReadAllLines(log).Should().HaveCount(3);
            File.ReadAllLines(log)[2].Should().Contain("specht.tool.1.2.3.nupkg")
                .And.Contain("Specht.CodeAnalysis.1.2.3.nupkg");

            File.Delete(analysis);
            File.Delete(log);
            var missing = await Run(scripts.Push, directory.FullName, bin, log);
            missing.ExitCode.Should().NotBe(0);
            File.Exists(log).Should().BeFalse();
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    private static (string Push, string Release) LoadReleaseScripts()
    {
        var root = FindRepositoryRoot();
        var stream = new YamlStream();
        using var input = File.OpenText(Path.Combine(root, ".github", "workflows", "release.yml"));
        stream.Load(input);
        var workflow = (YamlMappingNode)stream.Documents[0].RootNode;
        var jobs = (YamlMappingNode)workflow["jobs"];
        var release = (YamlMappingNode)jobs["release"];
        var steps = ((YamlSequenceNode)release["steps"]).Cast<YamlMappingNode>().ToArray();
        return (Script("Push"), Script("Release"));

        string Script(string name) => steps.Single(step => step["name"].ToString() == name)["run"].ToString();
    }

    private static async Task<(int ExitCode, string Output)> Run(string script, string directory, string bin, string log)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo("/bin/bash")
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        process.StartInfo.ArgumentList.Add("-e");
        process.StartInfo.ArgumentList.Add("-c");
        process.StartInfo.ArgumentList.Add(script);
        process.StartInfo.Environment["PATH"] = bin + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
        process.StartInfo.Environment["INVOCATIONS"] = log;
        process.StartInfo.Environment["GITHUB_REF_NAME"] = "v1.2.3";
        process.StartInfo.Environment["GITHUB_TOKEN"] = "fixture-token";
        process.StartInfo.Environment["GITHUB_REPOSITORY"] = "rlittlesii/specht";
        process.StartInfo.Environment["GH_TOKEN"] = "fixture-token";
        process.Start();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return (process.ExitCode, await stdout + await stderr);
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
