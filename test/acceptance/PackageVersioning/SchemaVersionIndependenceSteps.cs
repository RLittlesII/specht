using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;
using Reqnroll;

namespace Specht.Acceptance.PackageVersioning;

/// <summary>Proves 0055-F5 B-006 by building two commits differing only in the package version.</summary>
[Binding]
[Scope(Feature = "Package versioning")]
public sealed class SchemaVersionIndependenceSteps(PackedTool tool) : IDisposable
{
    [Given("a commit that changes only the package's version")]
    public async Task GivenACommitThatChangesOnlyThePackagesVersion()
    {
        await Run(_root, "git", "clone", "--shared", "--no-checkout", tool.Repository, "source");
        await Run(Source, "git", "checkout", "--detach", await tool.CommitAsync());
        await Run(Source, "git", "config", "user.name", "Version fixture");
        await Run(Source, "git", "config", "user.email", "version@example.invalid");
        var files = await Run(tool.Repository, "git", "ls-files", "--cached", "--others", "--exclude-standard", "-z");
        foreach (var file in files.Split('\0', StringSplitOptions.RemoveEmptyEntries))
        {
            var original = Path.Combine(tool.Repository, file);
            var copy = Path.Combine(Source, file);
            if (File.Exists(original))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(copy)!);
                File.Copy(original, copy, overwrite: true);
            }
            else
            {
                File.Delete(copy);
            }
        }
        await Run(Source, "git", "add", "--all");
        await Commit("Capture working build inputs");
        var path = Path.Combine(Source, "version.json");
        var version = JsonNode.Parse(await File.ReadAllTextAsync(path))!.AsObject();
        var current = Version.Parse(version["version"]!.GetValue<string>());
        version["version"] = $"{current.Major}.{current.Minor + 1}";
        await File.WriteAllTextAsync(path, version.ToJsonString());
        await Run(Source, "git", "add", "version.json");
        await Commit("Change package version");
        (await Run(Source, "git", "diff-tree", "--no-commit-id", "--name-only", "-r", "HEAD")).Trim().Should().Be("version.json");
        await File.WriteAllTextAsync(Path.Combine(_root, "nuget.config"), "<configuration><packageSources><clear /></packageSources></configuration>");
    }

    [When("both package versions are packed")]
    public async Task WhenBothPackageVersionsArePacked()
    {
        var changed = (await Run(Source, "git", "rev-parse", "HEAD")).Trim();
        await Run(Source, "git", "checkout", "--detach", "HEAD^");
        _before = await Pack("before");
        await Run(Source, "git", "checkout", "--detach", changed);
        _after = await Pack("after");
        _after.Value.Version.Should().NotBe(_before.Value.Version, "the fixture must change the actual package version");
    }

    [Then("the schema versions the tool embeds are unchanged")]
    public async Task ThenTheSchemaVersionsTheToolEmbedsAreUnchanged()
    {
        var before = await Run(_root, "dotnet", _before!.Value.Tool, "schema", "list", "--root", _root);
        before.Should().NotBeNullOrWhiteSpace();
        var after = await Run(_root, "dotnet", _after!.Value.Tool, "schema", "list", "--root", _root);
        after.Should().Be(before);
    }

    [Then("the schema version a new repository is initialised with is unchanged")]
    public async Task ThenTheSchemaVersionANewRepositoryIsInitialisedWithIsUnchanged()
    {
        var before = await Initialise(_before!.Value.Tool, "before");
        var after = await Initialise(_after!.Value.Tool, "after");
        before.Should().NotBeNullOrWhiteSpace();
        after.Should().Be(before);
    }

    /// <inheritdoc />
    public void Dispose() => Directory.Delete(_root, recursive: true);

    private static string Framework { get; } = "net" + new FrameworkName(
        typeof(SchemaVersionIndependenceSteps).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName).Version.ToString(2);

    private string Source => Path.Combine(_root, "source");

    private static async Task<string> Run(string directory, string command, params string[] arguments)
    {
        var start = new ProcessStartInfo(command)
        {
            WorkingDirectory = directory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["MSBUILDDISABLENODEREUSE"] = "1", ["NO_COLOR"] = "1", ["GITHUB_ACTIONS"] = "false" },
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        using var process = Process.Start(start) ?? throw new InvalidOperationException($"{command} did not start.");
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        process.ExitCode.Should().Be(0, await stdout + await stderr);

        return await stdout;
    }

    private Task<string> Commit(string message) =>
        Run(Source, "git", "-c", "core.hooksPath=", "-c", "commit.gpgSign=false", "commit", "--allow-empty", "-m", message);

    private async Task<(string Tool, string Version)> Pack(string name)
    {
        var artifacts = Path.Combine(_root, name);
        var packages = Path.Combine(artifacts, "packages");
        await Run(Source, "dotnet", "pack", "src/tool/tool.csproj", "--configuration", "Debug",
            "--artifacts-path", artifacts, "--output", packages, $"-p:TargetFrameworks={Framework}",
            $"-p:RestoreConfigFile={Path.Combine(_root, "nuget.config")}", "-p:NuGetAudit=false", "-p:RestoreEnablePackagePruning=false");
        var package = Directory.GetFiles(packages, "specht.tool.*.nupkg").Should().ContainSingle().Subject;
        using var archive = ZipFile.OpenRead(package);
        using var nuspec = archive.GetEntry("specht.tool.nuspec")!.Open();
        var version = XDocument.Load(nuspec).Descendants().Single(static element => element.Name.LocalName == "version").Value;
        var extracted = Path.Combine(artifacts, "extracted");
        archive.ExtractToDirectory(extracted);

        return (Path.Combine(extracted, "tools", Framework, "any", "specht.tool.dll"), version);
    }

    private async Task<string?> Initialise(string assembly, string name)
    {
        var consumer = Directory.CreateDirectory(Path.Combine(_root, "consumer-" + name)).FullName;
        await Run(_root, "dotnet", assembly, "init", "--root", consumer);
        var manifest = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(consumer, ".specht", "manifest.json")))!;

        return manifest["schemaVersion"]?.GetValue<string>();
    }

    private readonly string _root = Directory.CreateTempSubdirectory("specht-version-independence-").FullName;
    private (string Tool, string Version)? _before;
    private (string Tool, string Version)? _after;
}
