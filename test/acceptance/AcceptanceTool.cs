using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Specht.Tool.Features.Check;

namespace Specht.Acceptance;

/// <summary>
/// The built tool, launched as a shell launches it, so a scenario sees the process's stdout, stderr and exit code; Spectre's
/// command tester captures stdout alone (0001-F2 C-7). Any other process a scenario runs the same way, such as the build's
/// entry script and the pre-commit hook a check is called through (B-010, B-011). It also reads the version an assembly
/// states from the file's own metadata, the launched tool's (0001-F2 B-040) or one a package carries (B-030).
/// </summary>
internal static class AcceptanceTool
{
    /// <summary>The protocol revision the MCP supports (0132-F3 B-014).</summary>
    public const string McpRevision = "2026-07-28";

    /// <summary>The path of the built MCP program, beside the test assembly.</summary>
    public static string McpAssemblyPath { get; } = Path.Combine(AppContext.BaseDirectory, "specht.mcp.dll");

    /// <summary>Runs the tool in <paramref name="workingDirectory"/> with <paramref name="args"/> and waits for it to exit.</summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Launch(string workingDirectory, params string[] args) =>
        Launch(workingDirectory, new Dictionary<string, string>(StringComparer.Ordinal), args);

    /// <summary>
    /// Runs the tool in <paramref name="workingDirectory"/> with <paramref name="args"/>, its process carrying
    /// <paramref name="environment"/> over what this process hands it, and waits for it to exit.
    /// </summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="environment">The environment variables set for the process, by name.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Launch(
        string workingDirectory,
        IReadOnlyDictionary<string, string> environment,
        params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory };
        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        start.ArgumentList.Add(typeof(CheckCommand).Assembly.Location);
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        return Run(start);
    }

    /// <summary>Runs the tool with a complete document on its redirected standard input.</summary>
    /// <param name="workingDirectory">The process's working directory.</param>
    /// <param name="input">The document text to pipe.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>The process's stdout, stderr and exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) LaunchWithInput(string workingDirectory, string input, params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = workingDirectory };
        start.ArgumentList.Add(typeof(CheckCommand).Assembly.Location);
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        return Run(start, input);
    }

    /// <summary>Runs a process while draining both output streams and supplying its optional standard input before closing it.</summary>
    /// <param name="start">The process to run; its stdout and stderr are redirected here.</param>
    /// <param name="input">The optional text to write to redirected standard input.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Run(ProcessStartInfo start, string? input = null)
    {
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.RedirectStandardInput |= input is not null;

        using var process = Process.Start(start) ?? throw new InvalidOperationException($"{start.FileName} did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (start.RedirectStandardInput)
        {
            process.StandardInput.Write(input);
            process.StandardInput.Close();
        }

        var output = stdout.GetAwaiter().GetResult();
        var error = stderr.GetAwaiter().GetResult();
        process.WaitForExit();

        return (output, error, process.ExitCode);
    }

    /// <summary>
    /// Runs the MCP in <paramref name="workingDirectory"/> with <paramref name="args"/>, writes each of <paramref name="input"/> to its
    /// standard input as one line, closes the input and waits for it to exit.
    /// </summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="input">The lines the client sends, in order.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) LaunchMcp(string workingDirectory, IEnumerable<string> input, params string[] args)
    {
        using var session = McpSession.Start(workingDirectory, args);
        foreach (var line in input)
        {
            session.Send(line);
        }

        return session.Close();
    }

    /// <summary>Builds one request naming a protocol revision in its per-request metadata, as one line.</summary>
    /// <param name="id">The request id.</param>
    /// <param name="method">The method.</param>
    /// <param name="revision">The protocol revision the request names.</param>
    /// <returns>The JSON-RPC request.</returns>
    public static string McpRequest(int id, string method, string revision) =>
        new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = method,
            ["params"] = new JsonObject
            {
                ["_meta"] = new JsonObject
                {
                    ["io.modelcontextprotocol/protocolVersion"] = revision,
                    ["io.modelcontextprotocol/clientInfo"] = new JsonObject { ["name"] = "AcceptanceClient", ["version"] = "1.0.0" },
                    ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject(),
                },
            },
        }.ToJsonString();

    /// <summary>Builds the request a client of a revision with an initialize handshake opens with, as one line.</summary>
    /// <param name="id">The request id.</param>
    /// <returns>The JSON-RPC request.</returns>
    public static string McpLegacyInitialize(int id) =>
        new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = "initialize",
            ["params"] = new JsonObject
            {
                ["protocolVersion"] = "2025-11-25",
                ["capabilities"] = new JsonObject(),
                ["clientInfo"] = new JsonObject { ["name"] = "AcceptanceClient", ["version"] = "1.0.0" },
            },
        }.ToJsonString();

    /// <summary>Parses every line of <paramref name="stdout"/> as a JSON object.</summary>
    /// <param name="stdout">What a process wrote to its standard output.</param>
    /// <returns>The messages, in order.</returns>
    public static IReadOnlyList<JsonObject> McpMessages(string stdout) =>
        stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static line => JsonNode.Parse(line)!.AsObject())
            .ToList();

    /// <summary>Finds the answer to the request with <paramref name="id"/>.</summary>
    /// <param name="stdout">What a process wrote to its standard output.</param>
    /// <param name="id">The request id.</param>
    /// <returns>The message carrying that id, or <see langword="null"/> when none does.</returns>
    public static JsonObject? McpAnswer(string stdout, int id) =>
        McpMessages(stdout).SingleOrDefault(message => message["id"]?.GetValue<int>() == id);

    /// <summary>Records every file under <paramref name="root"/> as its root-relative path and the hash of its bytes.</summary>
    /// <param name="root">The directory to record.</param>
    /// <returns>The path to hash, with <c>/</c> separators.</returns>
    public static IReadOnlyDictionary<string, string> Snapshot(string root) =>
        Directory.GetFiles(root, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(root, path).Replace('\\', '/'),
                static path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),
                StringComparer.Ordinal);

    /// <summary>Reads the informational version the launched tool's assembly states.</summary>
    /// <returns>The version, or <see langword="null"/> when the assembly states none.</returns>
    public static string? InformationalVersion() => InformationalVersion(File.ReadAllBytes(typeof(CheckCommand).Assembly.Location));

    /// <summary>Reads the informational version an assembly states, from its metadata and without loading it.</summary>
    /// <param name="assembly">The assembly file's bytes.</param>
    /// <returns>The version, or <see langword="null"/> when the assembly states none.</returns>
    public static string? InformationalVersion(byte[] assembly)
    {
        using var portable = new PEReader(new MemoryStream(assembly));
        var reader = portable.GetMetadataReader();

        foreach (var handle in reader.GetAssemblyDefinition().GetCustomAttributes())
        {
            var attribute = reader.GetCustomAttribute(handle);
            if (attribute.Constructor.Kind == HandleKind.MemberReference
                && reader.GetMemberReference((MemberReferenceHandle) attribute.Constructor).Parent is { Kind: HandleKind.TypeReference } type
                && reader.GetString(reader.GetTypeReference((TypeReferenceHandle) type).Name) == nameof(AssemblyInformationalVersionAttribute))
            {
                var value = reader.GetBlobReader(attribute.Value);
                value.ReadUInt16();

                return value.ReadSerializedString();
            }
        }

        return null;
    }

    /// <summary>One run of the MCP's process, from its start to the close of its standard input.</summary>
    public sealed class McpSession : IDisposable
    {
        private McpSession(Process process)
        {
            _process = process;
            _stderr = process.StandardError.ReadToEndAsync();
        }

        /// <summary>The operating system's id of the process.</summary>
        public int ProcessId => _process.Id;

        /// <summary>Starts the MCP in <paramref name="workingDirectory"/> with <paramref name="args"/>, its standard input open.</summary>
        /// <param name="workingDirectory">The directory the process starts in.</param>
        /// <param name="args">The arguments, each passed as one.</param>
        /// <returns>The running session.</returns>
        public static McpSession Start(string workingDirectory, params string[] args)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            start.ArgumentList.Add(McpAssemblyPath);
            foreach (var arg in args)
            {
                start.ArgumentList.Add(arg);
            }

            return new McpSession(Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start."));
        }

        /// <summary>Writes one line to the process's standard input; a process that has already exited reads nothing.</summary>
        /// <param name="line">The line.</param>
        public void Send(string line)
        {
            try
            {
                _process.StandardInput.WriteLine(line);
                _process.StandardInput.Flush();
            }
            catch (IOException)
            {
            }
        }

        /// <summary>Reads the next line the process wrote to its standard output.</summary>
        /// <returns>The line, or <see langword="null"/> when the process closed its output or wrote none within the limit.</returns>
        public string? ReadLine()
        {
            string? line;
            try
            {
                line = _process.StandardOutput.ReadLineAsync().WaitAsync(Limit).GetAwaiter().GetResult();
            }
            catch (TimeoutException)
            {
                return null;
            }

            if (line is not null)
            {
                _stdout.AppendLine(line);
            }

            return line;
        }

        /// <summary>Lists the process's internet sockets, one line each, as <c>lsof</c> reports them.</summary>
        /// <param name="filter">The <c>lsof</c> selection arguments, such as <c>-iTCP -sTCP:LISTEN</c>.</param>
        /// <returns>The lines, with the header removed; empty when the process holds none.</returns>
        public IReadOnlyList<string> Sockets(params string[] filter)
        {
            var start = new ProcessStartInfo("lsof") { RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (var arg in new[] { "-nP", "-a", "-p", ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture) }.Concat(filter))
            {
                start.ArgumentList.Add(arg);
            }

            var (output, _, _) = Run(start);

            return output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Skip(1).ToList();
        }

        /// <summary>Closes the process's standard input, waits for it to exit and collects what it wrote.</summary>
        /// <returns>All of the process's stdout and stderr, and its exit code.</returns>
        public (string Stdout, string Stderr, int ExitCode) Close()
        {
            try
            {
                _process.StandardInput.Close();
            }
            catch (IOException)
            {
            }

            var rest = _process.StandardOutput.ReadToEndAsync();
            if (!_process.WaitForExit(Limit))
            {
                _process.Kill(entireProcessTree: true);
                throw new TimeoutException("The MCP did not exit after its standard input closed.");
            }

            _stdout.Append(rest.GetAwaiter().GetResult());

            return (_stdout.ToString(), _stderr.GetAwaiter().GetResult(), _process.ExitCode);
        }

        /// <summary>Ends the process if it still runs.</summary>
        public void Dispose()
        {
            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
            }

            _process.Dispose();
        }

        private static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

        private readonly Process _process;
        private readonly StringBuilder _stdout = new();
        private readonly Task<string> _stderr;
    }
}
