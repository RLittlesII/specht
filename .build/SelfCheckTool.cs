using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Specht.Build;

/// <summary>Prepares and invokes the working checkout through a locally installed package.</summary>
/// <param name="fileSystem">The build's file system.</param>
/// <param name="run">The asynchronous dotnet process boundary.</param>
public sealed class SelfCheckTool(IFileSystem fileSystem, Func<ProcessStartInfo, Task<int>> run)
{
    /// <summary>Returns the preparation failure or installed checker's exit code.</summary>
    /// <param name="root">The working or staged repository root.</param>
    /// <param name="configuration">The build configuration.</param>
    /// <returns>The first failure code, or zero.</returns>
    public async Task<int> RunAsync(string root, string configuration)
    {
        root = fileSystem.Path.GetFullPath(root);
        var workspace = fileSystem.Path.Combine(root, ".artifacts", "self-check");
        if (fileSystem.Directory.Exists(workspace))
        {
            fileSystem.Directory.Delete(workspace, recursive: true);
        }

        var feed = fileSystem.Path.Combine(workspace, "feed");
        fileSystem.Directory.CreateDirectory(feed);
        var pack = Command(root, "pack", "src/tool/tool.csproj", "--configuration", configuration, "--output", ".artifacts/self-check/feed");
        var code = await run(pack);
        if (code != 0)
        {
            return code;
        }

        var package = fileSystem.Directory.GetFiles(feed, "*.nupkg").Single();
        string version;
        using (var stream = fileSystem.File.OpenRead(package))
        using (var zip = new ZipArchive(stream))
        using (var metadata = zip.Entries.Single(static entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open())
        {
            var fields = XDocument.Load(metadata).Descendants().Single(static element => element.Name.LocalName == "metadata").Elements().ToArray();
            if (fields.Single(static element => element.Name.LocalName == "id").Value != "specht.tool")
            {
                throw new InvalidOperationException("The self-check package must be specht.tool.");
            }

            version = fields.Single(static element => element.Name.LocalName == "version").Value;
        }

        fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(workspace, ".config"));
        await fileSystem.File.WriteAllTextAsync(fileSystem.Path.Combine(workspace, ".config", "dotnet-tools.json"),
            "{\"version\":1,\"isRoot\":true,\"tools\":{}}\n");
        await fileSystem.File.WriteAllTextAsync(fileSystem.Path.Combine(workspace, "nuget.config"),
            "<configuration><packageSources><clear/><add key=\"local\" value=\"feed\"/></packageSources>"
            + "<packageSourceMapping><clear/></packageSourceMapping><auditSources><clear/></auditSources></configuration>\n");

        var install = LocalCommand(workspace, "tool", "install", "specht.tool", "--version", version, "--local",
            "--tool-manifest", ".config/dotnet-tools.json", "--configfile", "nuget.config", "--create-manifest-if-needed=false");
        code = await run(install);
        return code != 0 ? code : await run(LocalCommand(workspace, "tool", "run", "specht", "--", "--root", "../.."));
    }

    private static ProcessStartInfo Command(string directory, params string[] arguments)
    {
        var command = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            command.ArgumentList.Add(argument);
        }

        return command;
    }

    private ProcessStartInfo LocalCommand(string directory, params string[] arguments)
    {
        var command = Command(directory, arguments);
        command.Environment["DOTNET_CLI_HOME"] = fileSystem.Path.Combine(directory, "cli-home");
        command.Environment["NUGET_PACKAGES"] = fileSystem.Path.Combine(directory, "packages");
        command.Environment["NUGET_HTTP_CACHE_PATH"] = fileSystem.Path.Combine(directory, "http-cache");
        command.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        command.Environment["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1";
        return command;
    }
}
