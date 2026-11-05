using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
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
    /// standard input as one line and, after each line that is a request, reads its answer before the next line is sent, then
    /// closes the input and waits for it to exit. A line that is no request, such as noise, is written and awaits no answer.
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
            session.Exchange(line);
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
            ["params"] = new JsonObject { ["_meta"] = McpMeta(revision) },
        }.ToJsonString();

    /// <summary>Builds a call of the tool <paramref name="tool"/> with no arguments, naming the supported revision, as one line.</summary>
    /// <param name="id">The request id.</param>
    /// <param name="tool">The tool's name.</param>
    /// <returns>The JSON-RPC request.</returns>
    public static string McpToolCall(int id, string tool) =>
        new JsonObject
        {
            ["jsonrpc"] = "2.0",
            ["id"] = id,
            ["method"] = "tools/call",
            ["params"] = new JsonObject
            {
                ["name"] = tool,
                ["arguments"] = new JsonObject(),
                ["_meta"] = McpMeta(McpRevision),
            },
        }.ToJsonString();

    /// <summary>Reads why a tool call failed, as a client sees it: the protocol error, or the text of a result marked as an error.</summary>
    /// <param name="answer">The answer to a tool call.</param>
    /// <returns>The failure's text; <see langword="null"/> when the call did not fail.</returns>
    public static string? McpFailure(JsonObject answer) =>
        answer["error"] is { } error
            ? error.ToJsonString()
            : answer["result"]?["isError"]?.GetValue<bool>() == true
                ? string.Join('\n', McpTextBlocks(answer))
                : null;

    /// <summary>Lists the text of each content block of a tool call's result, in order.</summary>
    /// <param name="answer">The answer to a tool call.</param>
    /// <returns>The blocks' text.</returns>
    public static IReadOnlyList<string> McpTextBlocks(JsonObject answer) =>
        (answer["result"]?["content"] as JsonArray ?? [])
            .Select(static block => block?["text"]?.GetValue<string>() ?? string.Empty)
            .ToList();

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
        public static McpSession Start(string workingDirectory, params string[] args) =>
            Start(workingDirectory, new Dictionary<string, string>(StringComparer.Ordinal), args);

        /// <summary>
        /// Starts the MCP in <paramref name="workingDirectory"/> with <paramref name="args"/>, its standard input open and its process
        /// carrying <paramref name="environment"/> over what this process hands it.
        /// </summary>
        /// <param name="workingDirectory">The directory the process starts in.</param>
        /// <param name="environment">The environment variables set for the process, by name.</param>
        /// <param name="args">The arguments, each passed as one.</param>
        /// <returns>The running session.</returns>
        public static McpSession Start(string workingDirectory, IReadOnlyDictionary<string, string> environment, params string[] args)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (var (name, value) in environment)
            {
                start.Environment[name] = value;
            }

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

        /// <summary>
        /// Writes one line to the process's standard input and, when the line is a request, reads what the process writes until its
        /// answer arrives, as a client that waits for an answer before it sends the next line or closes its input.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns>The answer's line; <see langword="null"/> when the line is no request or the process closed its output first.</returns>
        /// <exception cref="TimeoutException">The process wrote nothing within the limit.</exception>
        public string? Exchange(string line)
        {
            Send(line);
            if (RequestId(line) is not { } id)
            {
                return null;
            }

            while (ReadLine() is { } answer)
            {
                if (IsAnswerTo(answer, id))
                {
                    return answer;
                }
            }

            return null;
        }

        /// <summary>
        /// Calls the tool <paramref name="tool"/> as a client does: it first reads what the host offers and fails the test when the tool is
        /// not offered, so a host that has not registered the tool is red for that reason and not green for the error a missing tool is.
        /// </summary>
        /// <param name="id">The request id of the call; the listing takes the id after it.</param>
        /// <param name="tool">The tool's name.</param>
        /// <returns>The answer to the call.</returns>
        /// <exception cref="Xunit.Sdk.XunitException">The host does not offer the tool, or answered the call with nothing.</exception>
        public JsonObject CallTool(int id, string tool)
        {
            var listing = Exchange(McpRequest(id + 1, "tools/list", McpRevision));
            var offered = Parse(listing ?? string.Empty)?["result"]?["tools"] is JsonArray tools
                && tools.Any(offer => offer?["name"]?.GetValue<string>() == tool);
            if (!offered)
            {
                throw new Xunit.Sdk.XunitException($"The host offers no '{tool}' tool; tools/list answered: {listing ?? "nothing"}");
            }

            var answer = Exchange(McpToolCall(id, tool));

            return Parse(answer ?? string.Empty) ?? throw new Xunit.Sdk.XunitException($"The host answered the call of '{tool}' with nothing.");
        }

        /// <summary>Reads the next line the process wrote to its standard output.</summary>
        /// <returns>The line, or <see langword="null"/> when the process closed its output.</returns>
        /// <exception cref="TimeoutException">The process wrote nothing within the limit; the process is ended.</exception>
        public string? ReadLine()
        {
            string? line;
            try
            {
                line = _process.StandardOutput.ReadLineAsync().WaitAsync(Limit).GetAwaiter().GetResult();
            }
            catch (TimeoutException)
            {
                _process.Kill(entireProcessTree: true);
                throw new TimeoutException("The MCP wrote nothing within the limit.");
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

        private static int? RequestId(string line) =>
            Parse(line) is { } message
            && message.ContainsKey("method")
            && message["id"] is JsonValue id
            && id.TryGetValue<int>(out var value)
                ? value
                : null;

        private static bool IsAnswerTo(string line, int id) =>
            Parse(line) is { } message
            && !message.ContainsKey("method")
            && message["id"] is JsonValue value
            && value.TryGetValue<int>(out var found)
            && found == id;

        private static JsonObject? Parse(string line)
        {
            try
            {
                return JsonNode.Parse(line) as JsonObject;
            }
            catch (System.Text.Json.JsonException)
            {
                return null;
            }
        }

        private static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

        private readonly Process _process;
        private readonly StringBuilder _stdout = new();
        private readonly Task<string> _stderr;
    }

    /// <summary>
    /// A world of fake <c>specht</c> tools for one session of the MCP (0132-F3 B-017 to B-030, B-034): a pinned one restored by a real
    /// <c>dotnet tool restore</c> from a local feed into folders of its own, a global one, and one reachable from the path. Each fake
    /// appends a line to a log outside every root, as its label, its version and its arguments, so a test reads which tool ran, how many
    /// version checks it was given and what work. Nothing here reaches the network, the user's home or a real <c>specht</c>; the fakes are
    /// shell scripts, so a test that uses them skips on Windows.
    /// </summary>
    public sealed class FakeTools : IDisposable
    {
        /// <summary>Creates the folders of one world; nothing is installed.</summary>
        public FakeTools()
        {
            _directory = Path.Combine(Path.GetTempPath(), "specht-fake-tools-" + Guid.NewGuid().ToString("N"));
            _home = Path.Combine(_directory, "home");
            _packages = Path.Combine(_directory, "packages");
            _feed = Path.Combine(_directory, "feed");
            _globalBin = Path.Combine(_home, ".dotnet", "tools");
            _pathBin = Path.Combine(_directory, "pathbin");
            _dotnetBin = Path.Combine(_directory, "dotnetbin");
            _log = Path.Combine(_directory, "runs.log");
            foreach (var folder in new[] { _home, _packages, _feed, _globalBin, _pathBin, _dotnetBin })
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(_log, string.Empty);
            if (!OperatingSystem.IsWindows())
            {
                File.CreateSymbolicLink(
                    Path.Combine(_dotnetBin, "dotnet"),
                    Path.GetFullPath(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "..", "..", "..", "dotnet")));
            }
        }

        /// <summary>The version a fake pinned tool states unless a test says otherwise; far above any minimum the MCP can require.</summary>
        public const string PinnedVersion = "98.76.54";

        /// <summary>The version of the fake reachable from the path.</summary>
        public const string PathVersion = "97.65.43";

        /// <summary>The version of the fake global tool.</summary>
        public const string GlobalVersion = "96.54.32";

        /// <summary>A version older than any minimum the MCP can require.</summary>
        public const string OldVersion = "0.0.1";

        /// <summary>What a fake writes on standard output for any command but <c>--version</c>, followed by its label.</summary>
        public const string OutputMarker = "FAKE-SPECHT-OUTPUT";

        /// <summary>The line in a specification that makes a fake exit <c>1</c>, as a check does on a violation.</summary>
        public const string Violation = "FAKE-VIOLATION";

        /// <summary>The label of the fake restored from the local tool manifest's pin.</summary>
        public const string Pinned = "pinned";

        /// <summary>The label of the fake global tool.</summary>
        public const string Global = "global";

        /// <summary>The label of the fake reachable from the path.</summary>
        public const string OnPath = "path";

        /// <summary>Gets the environment a process runs in to see this world and nothing else of the machine's tools.</summary>
        public IReadOnlyDictionary<string, string> Environment =>
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["PATH"] = string.Join(Path.PathSeparator, _pathBin, _globalBin, _dotnetBin, "/usr/bin", "/bin"),
                ["HOME"] = _home,
                ["DOTNET_CLI_HOME"] = _home,
                ["NUGET_PACKAGES"] = _packages,
                ["SPECHT_FAKE_LOG"] = _log,
                ["DOTNET_NOLOGO"] = "1",
                ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
            };

        /// <summary>Gets every run of a fake so far, in order.</summary>
        public IReadOnlyList<FakeRun> Runs =>
            File.ReadAllLines(_log)
                .Where(static line => line.Length > 0)
                .Select(static line => line.Split(' ', 3))
                .Select(static parts => new FakeRun(parts[0], parts[1], parts.Length > 2 ? parts[2] : string.Empty))
                .ToList();

        /// <summary>Gets whether anything of the pinned tool is in the folders a restore writes to.</summary>
        public bool AnythingRestored =>
            Directory.EnumerateFileSystemEntries(_packages).Any()
            || Directory.Exists(Path.Combine(_home, ".dotnet", "toolResolverCache"));

        /// <summary>
        /// Writes two specifications under <paramref name="root"/>, one of which carries the line a fake reports as a violation, the
        /// way a repository of two Feature specifications with a violation looks to a fake.
        /// </summary>
        /// <param name="root">The root to write under.</param>
        public static void WriteSpecifications(string root)
        {
            WriteFile(Path.Combine(root, "features", "clean", ".spec", "README.md"), "# Clean\n\nA specification the check accepts \u2014 caf\u00e9.\n");
            WriteFile(Path.Combine(root, "features", "violating", ".spec", "README.md"), $"# Violating\n\n{Violation} in this specification.\n");
        }

        /// <summary>Rewrites the violating specification without the line a fake reports.</summary>
        /// <param name="root">The root the specifications were written under.</param>
        public static void FixViolation(string root) =>
            WriteFile(Path.Combine(root, "features", "violating", ".spec", "README.md"), "# Violating\n\nFixed.\n");

        /// <summary>
        /// Gets what a fake labelled <paramref name="label"/> writes on standard output for <c>--json</c> on <paramref name="root"/>: its
        /// marker, then the root's manifest and every specification under it, in path order.
        /// </summary>
        /// <param name="label">Which fake ran.</param>
        /// <param name="root">The root it ran on.</param>
        /// <returns>The text, byte for byte.</returns>
        public static string JsonOutput(string label, string root) =>
            $"{OutputMarker} {label} json\n"
            + File.ReadAllText(Path.Combine(root, ".specht", "manifest.json"))
            + string.Concat(
                Directory.EnumerateFiles(root, "README.md", SearchOption.AllDirectories)
                    .Select(path => "./" + Path.GetRelativePath(root, path).Replace('\\', '/'))
                    .Where(static path => path.EndsWith("/.spec/README.md", StringComparison.Ordinal))
                    .OrderBy(static path => path, StringComparer.Ordinal)
                    .Select(path => File.ReadAllText(Path.Combine(root, path))));

        /// <summary>
        /// Writes a local tool manifest in <paramref name="directory"/> listing <c>specht.tool</c> at <paramref name="version"/>, a feed
        /// that holds the fake package and a NuGet configuration that reads only that feed, so a restore the host should not run could
        /// succeed. Restores the fake when <paramref name="restored"/>.
        /// </summary>
        /// <param name="directory">The directory that holds the manifest, the root or a directory above it.</param>
        /// <param name="version">The pinned version, which the fake states.</param>
        /// <param name="restored">Whether to run the restore the repository's own build would.</param>
        public void Pin(string directory, string version, bool restored)
        {
            WriteFile(
                Path.Combine(directory, ".config", "dotnet-tools.json"),
                "{\"version\":1,\"isRoot\":true,\"tools\":{\"specht.tool\":{\"version\":\"" + version + "\",\"commands\":[\"specht\"]}}}");
            WriteFile(
                Path.Combine(directory, "nuget.config"),
                "<configuration><packageSources><clear /><add key=\"fake\" value=\"" + _feed + "\" /></packageSources></configuration>");
            using (var package = new ZipArchive(File.Create(Path.Combine(_feed, $"specht.tool.{version}.nupkg")), ZipArchiveMode.Create))
            {
                AddEntry(
                    package,
                    "specht.tool.nuspec",
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?><package xmlns=\"http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd\"><metadata>"
                    + $"<id>specht.tool</id><version>{version}</version><authors>fake</authors><description>fake</description>"
                    + "<packageTypes><packageType name=\"DotnetTool\" /></packageTypes></metadata></package>");
                AddEntry(
                    package,
                    "tools/any/any/DotnetToolSettings.xml",
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?><DotNetCliTool Version=\"1\"><Commands>"
                    + "<Command Name=\"specht\" EntryPoint=\"specht.sh\" Runner=\"executable\" /></Commands></DotNetCliTool>");
                AddEntry(package, "tools/any/any/specht.sh", Script(Pinned, version));
            }

            if (restored)
            {
                Restore(directory);
            }
        }

        /// <summary>Removes the pin: deletes the manifest in <paramref name="directory"/>, or leaves one that lists no tool.</summary>
        /// <param name="directory">The directory that holds the manifest.</param>
        /// <param name="removed">Whether the manifest is deleted rather than emptied.</param>
        public void Unpin(string directory, bool removed)
        {
            var manifest = Path.Combine(directory, ".config", "dotnet-tools.json");
            if (removed)
            {
                File.Delete(manifest);
            }
            else
            {
                WriteFile(manifest, "{\"version\":1,\"isRoot\":true,\"tools\":{}}");
            }
        }

        /// <summary>Removes the restored copy, as a clone that has not run the restore has none.</summary>
        public void Unrestore()
        {
            Directory.Delete(_packages, recursive: true);
            Directory.CreateDirectory(_packages);
            var cache = Path.Combine(_home, ".dotnet", "toolResolverCache");
            if (Directory.Exists(cache))
            {
                Directory.Delete(cache, recursive: true);
            }
        }

        /// <summary>Installs a fake global tool, the way a global install puts a command in the folder on the path.</summary>
        /// <param name="version">The version it states.</param>
        public void InstallGlobal(string version) => WriteScript(_globalBin, Global, version);

        /// <summary>Puts a fake <c>specht</c> in a folder at the front of the path.</summary>
        /// <param name="version">The version it states.</param>
        public void InstallOnPath(string version) => WriteScript(_pathBin, OnPath, version);

        /// <summary>Deletes the world.</summary>
        public void Dispose()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, recursive: true);
            }
        }

        private static string Script(string label, string version) =>
            "#!/bin/sh\n"
            + $"echo \"{label} {version} $*\" >> \"$SPECHT_FAKE_LOG\"\n"
            + $"if [ \"$1\" = \"--version\" ]; then echo {version}; exit 0; fi\n"
            + "case \" $* \" in *\" --json \"*) ;; *) echo " + OutputMarker + $" {label}; exit 0;; esac\n"
            + "root=\"$PWD\"; previous=\"\"\n"
            + "for argument in \"$@\"; do if [ \"$previous\" = \"--root\" ]; then root=\"$argument\"; fi; previous=\"$argument\"; done\n"
            + $"echo \"{OutputMarker} {label} json\"\n"
            + "cat \"$root/.specht/manifest.json\"\n"
            + "(cd \"$root\" && find . -path '*/.spec/README.md' | LC_ALL=C sort | while read -r file; do cat \"$file\"; done)\n"
            + $"if grep -rqs --include=README.md {Violation} \"$root\"; then exit 1; fi\n"
            + "exit 0\n";

        private static void AddEntry(ZipArchive package, string name, string content)
        {
            using var writer = new StreamWriter(package.CreateEntry(name).Open());
            writer.Write(content);
        }

        private static void WriteFile(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        private static void WriteScript(string folder, string label, string version)
        {
            var path = Path.Combine(folder, "specht");
            File.WriteAllText(path, Script(label, version));
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            }
        }

        private void Restore(string directory)
        {
            var start = new ProcessStartInfo("dotnet") { WorkingDirectory = directory };
            start.ArgumentList.Add("tool");
            start.ArgumentList.Add("restore");
            foreach (var (name, value) in Environment)
            {
                start.Environment[name] = value;
            }

            var (stdout, stderr, exitCode) = Run(start);
            if (exitCode != 0)
            {
                throw new InvalidOperationException($"The fake tool did not restore. Exit {exitCode}. {stdout}{stderr}");
            }
        }

        private readonly string _directory;
        private readonly string _home;
        private readonly string _packages;
        private readonly string _feed;
        private readonly string _globalBin;
        private readonly string _pathBin;
        private readonly string _dotnetBin;
        private readonly string _log;
    }

    /// <summary>One run of a fake <c>specht</c>.</summary>
    /// <param name="Label">Which fake ran: pinned, global or path.</param>
    /// <param name="Version">The version that fake states.</param>
    /// <param name="Arguments">The arguments it was run with, joined by a space.</param>
    public sealed record FakeRun(string Label, string Version, string Arguments)
    {
        /// <summary>Gets whether the run asked the tool for its version.</summary>
        public bool IsVersionCheck => Arguments == "--version";
    }

    private static JsonObject McpMeta(string revision) =>
        new()
        {
            ["io.modelcontextprotocol/protocolVersion"] = revision,
            ["io.modelcontextprotocol/clientInfo"] = new JsonObject { ["name"] = "AcceptanceClient", ["version"] = "1.0.0" },
            ["io.modelcontextprotocol/clientCapabilities"] = new JsonObject(),
        };
}
