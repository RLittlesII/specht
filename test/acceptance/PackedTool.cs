using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using AwesomeAssertions;

namespace Specht.Acceptance;

/// <summary>
/// The tool as the build packs it, packed once for the whole test run and read by every scenario that says "the tool is
/// packed" (0055-F5 B-005; 0001-F2 B-017 to B-029). It issues the command the build's <c>Pack</c> target issues -
/// <c>dotnet pack</c> on the solution, already compiled, in the configuration this assembly was built in - into a temporary
/// directory instead of the build's package output, so it never runs the build from inside the build's own test target.
/// One pack, because two packs of one solution at a time write the same intermediate files. The package is found as the
/// <c>specht.tool</c> package among the solution's outputs and its manifest as the one <c>.nuspec</c> at its root:
/// neither is opened by a name, so a scenario about what the tool package says does not fail on its file name.
/// </summary>
public sealed class PackedTool : IDisposable
{
    /// <summary>Initializes a new instance of the <see cref="PackedTool"/> class; nothing is packed until a scenario asks.</summary>
    public PackedTool() => _package = new Lazy<Task<string>>(PackAsync);

    /// <summary>Gets the root of the repository this assembly was built in, the directory every build here is made in.</summary>
    public string Repository { get; } = FindRepository(AppContext.BaseDirectory);

    /// <summary>Packs the tool, the first time it is asked.</summary>
    /// <returns>The path of the one package the pack wrote.</returns>
    public Task<string> PackageAsync() => _package.Value;

    /// <summary>Gets the analysis package produced by the same solution pack.</summary>
    /// <returns>The path of the one <c>Specht.CodeAnalysis</c> package.</returns>
    public async Task<string> CodeAnalysisPackageAsync()
    {
        _ = await _package.Value;
        return Directory.GetFiles(_packages!, "Specht.CodeAnalysis.*.nupkg").Should().ContainSingle().Subject;
    }

    /// <summary>Reads what the package says about itself.</summary>
    /// <returns>The <c>metadata</c> element of the package's manifest.</returns>
    public async Task<XElement> MetadataAsync()
    {
        using var archive = ZipFile.OpenRead(await PackageAsync());
        var manifest = archive.Entries
            .Where(static entry => !entry.FullName.Contains('/') && entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal))
            .Should().ContainSingle().Subject;
        using var stream = manifest.Open();

        return XDocument.Load(stream).Root!.Elements().Single(static element => element.Name.LocalName == "metadata");
    }

    /// <summary>Reads a file the package carries.</summary>
    /// <param name="path">The file's path inside the package, as the package's manifest gives it.</param>
    /// <returns>The file's bytes, or <see langword="null"/> when the package carries no file at that path.</returns>
    public async Task<byte[]?> FileAsync(string path)
    {
        using var archive = ZipFile.OpenRead(await PackageAsync());
        var entry = archive.GetEntry(path.Replace('\\', '/'));
        if (entry is null)
        {
            return null;
        }

        using var stream = entry.Open();
        using var bytes = new MemoryStream();
        await stream.CopyToAsync(bytes);

        return bytes.ToArray();
    }

    /// <summary>Asks the <c>nbgv</c> tool which commit this repository has checked out.</summary>
    /// <returns>The commit's full id.</returns>
    public async Task<string> CommitAsync()
    {
        var (stdout, stderr, exitCode) = await DotNetAsync("nbgv", "get-version", "--variable", "GitCommitId");
        exitCode.Should().Be(0, stdout + stderr);

        return stdout.Trim();
    }

    /// <summary>Lists what the package carries.</summary>
    /// <returns>The path of every file inside the package.</returns>
    public async Task<IReadOnlyList<string>> FilesAsync()
    {
        using var archive = ZipFile.OpenRead(await PackageAsync());

        return [.. archive.Entries.Select(static entry => entry.FullName)];
    }

    /// <summary>
    /// Builds the tool as a continuous-integration run builds it: the same <c>dotnet build</c>, under the two environment
    /// variables a GitHub Actions runner gives every step, <c>GITHUB_ACTIONS=true</c> and a <c>GITHUB_ENV</c> file. It
    /// builds into a directory of its own under the repository's <c>.artifacts</c>, so the build the test run stands on is
    /// not touched, and inside the repository, where a runner's build output is.
    /// </summary>
    /// <returns>A task that completes when the build has.</returns>
    public async Task BuildAsContinuousIntegrationAsync()
    {
        _built = Path.Combine(Repository, ".artifacts", "acceptance", "continuous-integration-" + Path.GetRandomFileName());
        Directory.CreateDirectory(_built);
        var (stdout, stderr, exitCode) = await DotNetAsync(
            Runner(_built), "build", Tool, "--configuration", Configuration, "--artifacts-path", _built);
        exitCode.Should().Be(0, stdout + stderr);
    }

    /// <summary>Packs the build <see cref="BuildAsContinuousIntegrationAsync"/> made, under the same environment.</summary>
    /// <returns>The path of the one package the pack wrote.</returns>
    public async Task<string> PackThatBuildAsync()
    {
        var built = _built ?? throw new InvalidOperationException("The tool was not built.");
        var packages = Path.Combine(built, "package");
        var (stdout, stderr, exitCode) = await DotNetAsync(
            Runner(built), "pack", Tool, "--configuration", Configuration, "--no-build", "--artifacts-path", built, "--output", packages);
        exitCode.Should().Be(0, stdout + stderr);

        return Directory.GetFiles(packages).Should().ContainSingle().Subject;
    }

    /// <summary>Runs <c>dotnet</c> from the root of the repository this assembly was built in and waits for it to exit.</summary>
    /// <param name="arguments">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public Task<(string Stdout, string Stderr, int ExitCode)> DotNetAsync(params string[] arguments) => DotNetAsync([], arguments);

    /// <inheritdoc />
    public void Dispose()
    {
        if (_packages is not null)
        {
            Directory.Delete(_packages, recursive: true);
        }

        if (_built is not null)
        {
            Directory.Delete(_built, recursive: true);
        }
    }

    private static string Configuration { get; } =
        typeof(PackedTool).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
        ?? throw new InvalidOperationException("The test assembly records no build configuration.");

    private static Dictionary<string, string> Runner(string built) =>
        new() { ["GITHUB_ACTIONS"] = "true", ["GITHUB_ENV"] = Path.Combine(built, "environment") };

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                             ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private async Task<string> PackAsync()
    {
        _packages = Directory.CreateTempSubdirectory("specht-pack-").FullName;
        var (stdout, stderr, exitCode) = await DotNetAsync(
            "pack", "specht.slnx", "--configuration", Configuration, "--no-build", "--output", _packages);
        exitCode.Should().Be(0, stdout + stderr);

        return Directory.GetFiles(_packages, "specht.tool.*.nupkg").Should().ContainSingle().Subject;
    }

    private async Task<(string Stdout, string Stderr, int ExitCode)> DotNetAsync(Dictionary<string, string> environment, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Repository,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["MSBUILDDISABLENODEREUSE"] = "1", ["NO_COLOR"] = "1" },
        };

        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return (await stdout, await stderr, process.ExitCode);
    }

    private const string Tool = "src/tool/tool.csproj";

    private readonly Lazy<Task<string>> _package;
    private string? _packages;
    private string? _built;
}
