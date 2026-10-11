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
/// one file the pack wrote and its manifest as the one <c>.nuspec</c> at its root: neither is opened by a name, so a
/// scenario about what the package says does not fail on what the package is called.
/// </summary>
public sealed class PackedTool : IDisposable
{
    /// <summary>Initializes a new instance of the <see cref="PackedTool"/> class; nothing is packed until a scenario asks.</summary>
    public PackedTool() => _package = new Lazy<Task<string>>(PackAsync);

    /// <summary>Packs the tool, the first time it is asked.</summary>
    /// <returns>The path of the one package the pack wrote.</returns>
    public Task<string> PackageAsync() => _package.Value;

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

    /// <summary>Runs <c>dotnet</c> from the root of the repository this assembly was built in and waits for it to exit.</summary>
    /// <param name="arguments">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public async Task<(string Stdout, string Stderr, int ExitCode)> DotNetAsync(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = _repository,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment = { ["MSBUILDDISABLENODEREUSE"] = "1", ["NO_COLOR"] = "1" },
        };

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

    /// <inheritdoc />
    public void Dispose()
    {
        if (_packages is not null)
        {
            Directory.Delete(_packages, recursive: true);
        }
    }

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                             ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private async Task<string> PackAsync()
    {
        var configuration = typeof(PackedTool).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration
                            ?? throw new InvalidOperationException("The test assembly records no build configuration.");
        _packages = Directory.CreateTempSubdirectory("specht-pack-").FullName;
        var (stdout, stderr, exitCode) = await DotNetAsync(
            "pack", "specht.slnx", "--configuration", configuration, "--no-build", "--output", _packages);
        exitCode.Should().Be(0, stdout + stderr);

        return Directory.GetFiles(_packages).Should().ContainSingle().Subject;
    }

    private readonly string _repository = FindRepository(AppContext.BaseDirectory);
    private readonly Lazy<Task<string>> _package;
    private string? _packages;
}
