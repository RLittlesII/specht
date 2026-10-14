using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Specht.Benchmarks.Startup;

internal sealed class ColdStartSession : IDisposable
{
    private ColdStartSession(string repository, string dotnet)
    {
        _repository = repository;
        _dotnet = dotnet;
        _root = Directory.CreateTempSubdirectory("specht-startup-").FullName;
    }

    public static async Task<ColdStartSession> CreateAsync()
    {
        var repository = FindRepository();
        var session = new ColdStartSession(repository, FindDotNet());
        try
        {
            await session.InstallAsync();
            return session;
        }
        catch (Exception exception)
        {
            session.Dispose();
            throw new InvalidOperationException($"Cold-start setup failed: {session.Sanitize(exception.Message)}");
        }
    }

    public Task<string> RunAsync(string command, params string[] arguments) =>
        DotNetAsync(command, ["tool", "run", command, "--", .. arguments]);

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    private static string FindRepository()
    {
        for (var directory = new FileInfo(typeof(ColdStartSession).Assembly.Location).Directory;
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "build.sh")) &&
                File.Exists(Path.Combine(directory.FullName, ".performance", "benchmarks", "benchmarks.csproj")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("No benchmark repository above the assembly; run the Benchmark target.");
    }

    private static string FindDotNet()
    {
        var host = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");
        if (host is not null && File.Exists(host))
        {
            return host;
        }

        if (Environment.ProcessPath is { } process && Path.GetFileNameWithoutExtension(process) == "dotnet")
        {
            return process;
        }

        var name = OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet";
        return (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                   .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                   .Select(path => Path.Combine(path, name))
                   .FirstOrDefault(File.Exists)
               ?? throw new InvalidOperationException("No dotnet executable available for the cold-start session.");
    }

    private static (string Path, string Version) ReadPackage(string path)
    {
        using var package = ZipFile.OpenRead(path);
        var manifest = package.Entries.Single(static entry =>
            !entry.FullName.Contains('/') && entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
        using var stream = manifest.Open();
        var metadata = XDocument.Load(stream).Root!.Elements().Single(static element => element.Name.LocalName == "metadata");
        var id = metadata.Elements().Single(static element => element.Name.LocalName == "id").Value;
        var version = metadata.Elements().Single(static element => element.Name.LocalName == "version").Value;
        return (string.Equals(id, "specht.tool", StringComparison.OrdinalIgnoreCase) ? path : string.Empty, version);
    }

    private async Task InstallAsync()
    {
        foreach (var (name, directory) in new Dictionary<string, string>
        {
            ["DOTNET_CLI_HOME"] = "cli-home",
            ["NUGET_PACKAGES"] = "packages",
            ["NUGET_HTTP_CACHE_PATH"] = "http-cache",
            ["NUGET_PLUGINS_CACHE_PATH"] = "plugins-cache",
            ["NUGET_SCRATCH"] = "scratch",
            ["TMPDIR"] = "temp",
            ["TMP"] = "temp",
            ["TEMP"] = "temp",
        })
        {
            _environment[name] = Directory.CreateDirectory(Path.Combine(_root, directory)).FullName;
        }

        var package = Directory.EnumerateFiles(Path.Combine(_repository, ".artifacts", "nupkg"), "*.nupkg")
            .Select(ReadPackage)
            .Where(static package => package.Path.Length > 0)
            .ToArray();
        if (package.Length != 1)
        {
            throw new InvalidOperationException("Expected exactly one packed specht.tool package; run the Benchmark target.");
        }

        var source = Directory.CreateDirectory(Path.Combine(_root, "source")).FullName;
        File.Copy(package[0].Path, Path.Combine(source, Path.GetFileName(package[0].Path)));
        File.Copy(Path.Combine(_repository, "global.json"), Path.Combine(_root, "global.json"));
        Directory.CreateDirectory(Path.Combine(_root, ".config"));
        await File.WriteAllTextAsync(Path.Combine(_root, ".config", "dotnet-tools.json"),
            "{\"version\":1,\"isRoot\":true,\"tools\":{}}\n");
        await File.WriteAllTextAsync(Path.Combine(_root, "nuget.config"),
            "<configuration><packageSources><clear/><add key=\"local\" value=\"source\"/></packageSources></configuration>\n");
        var baseline = Directory.CreateDirectory(Path.Combine(_root, "baseline")).FullName;
        await File.WriteAllTextAsync(Path.Combine(baseline, "empty.csproj"), string.Join('\n',
            "<Project Sdk=\"Microsoft.NET.Sdk\">",
            "  <PropertyGroup>",
            "    <OutputType>Exe</OutputType>",
            "    <TargetFramework>net10.0</TargetFramework>",
            "    <PackAsTool>true</PackAsTool>",
            "    <ToolCommandName>specht-empty</ToolCommandName>",
            "    <PackageId>specht.benchmark.empty</PackageId>",
            "    <Version>1.0.0</Version>",
            "  </PropertyGroup>",
            "</Project>", string.Empty));
        await File.WriteAllTextAsync(Path.Combine(baseline, "Program.cs"), "return 0;\n");
        await DotNetAsync("baseline restore", "restore", "baseline/empty.csproj", "--configfile", "nuget.config", "-p:NuGetAudit=false");
        await DotNetAsync("baseline pack", "pack", "baseline/empty.csproj", "--no-restore", "--configuration", "Release", "--output", "source",
            "-p:UseSharedCompilation=false");
        foreach (var (id, version) in new[] { ("specht.tool", package[0].Version), ("specht.benchmark.empty", "1.0.0") })
        {
            await DotNetAsync("local install", "tool", "install", id, "--version", version, "--local",
                "--tool-manifest", ".config/dotnet-tools.json", "--configfile", "nuget.config", "--create-manifest-if-needed=false");
        }

        new SpechtTreeStore(new FileSystem()).Save(SpechtTree.Of(1), Path.Combine(_root, "tree"));
        await RunAsync("specht-empty");
        await RunAsync("specht", "--help");
        using var report = JsonDocument.Parse(await RunAsync("specht", "--root", "tree", "--json"));
        var count = report.RootElement.GetProperty("layouts").EnumerateArray()
            .Sum(static layout => layout.GetProperty("specificationCount").GetInt32());
        if (count != 1 || report.RootElement.GetProperty("violations").GetArrayLength() != 0)
        {
            throw new InvalidOperationException("Cold-start input must contain exactly one clean specification.");
        }
    }

    private async Task<string> DotNetAsync(string stage, params string[] arguments)
    {
        var start = new ProcessStartInfo(_dotnet)
        {
            WorkingDirectory = _root,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var (name, value) in _environment)
        {
            start.Environment[name] = value;
        }

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Cold-start {stage} did not start.");
        process.StandardInput.Close();
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = await stdout;
        var error = await stderr;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Cold-start {stage} exited {process.ExitCode}: {Sanitize(output + error)}");
        }

        return output;
    }

    private string Sanitize(string text) =>
        text.Replace(_root, "<session>", StringComparison.OrdinalIgnoreCase)
            .Replace(_repository, ".", StringComparison.OrdinalIgnoreCase);

    private readonly string _repository;
    private readonly string _dotnet;
    private readonly string _root;
    private readonly Dictionary<string, string> _environment = new()
    {
        ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
        ["DOTNET_NOLOGO"] = "1",
        ["DOTNET_ADD_GLOBAL_TOOLS_TO_PATH"] = "0",
        ["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "true",
        ["MSBUILDDISABLENODEREUSE"] = "1",
    };
}
