using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// Runs the built MCP as a process and holds what the host does over its standard input and output (0132-F3 B-001 to B-006,
/// B-008, B-010 to B-014, B-019, B-032, B-033). The scenarios of the pinned-tool resolution (0359) and the documentation (0332)
/// have no binding here, and B-009's wait for the first answer the tool serves (0330).
/// </summary>
[Binding]
[Scope(Feature = "The MCP host")]
public sealed class McpHostSteps
{
    [Given("a repository whose local tool manifest lists the tool")]
    public void GivenAToolManifest()
    {
        _tree = new SpechtTree();
        Tree.WriteRaw(
            ".config/dotnet-tools.json",
            "{\"version\":1,\"isRoot\":true,\"tools\":{\"specht.tool\":{\"version\":\"0.0.0\",\"commands\":[\"specht\"]}}}");
    }

    [Given("the pinned tool is restored")]
    public void GivenTheToolIsRestored()
    {
    }

    [Given("a manifest the check accepts")]
    public void GivenAnAcceptedManifest() => File.Exists(Path.Combine(Tree.Root, SpechtTree.ManifestPath)).Should().BeTrue();

    [Given(@"^a client configured with one entry that runs the MCP (through the local tool manifest|from a global install)$")]
    public void GivenAnEntry(string installed) => installed.Should().NotBeNullOrWhiteSpace();

    [Given(@"^the repository's manifest (is removed|is replaced by one the check rejects)$")]
    public void GivenAnUnusableManifest(string state)
    {
        if (state == "is removed")
        {
            File.Delete(Path.Combine(Tree.Root, SpechtTree.ManifestPath));
        }
        else
        {
            Tree.WriteRaw(SpechtTree.ManifestPath, "{\"schemaVersion\":\"9.0.0\"}");
        }
    }

    [Given("the client names a root that is not a directory")]
    public void GivenANonDirectoryRoot() => _arguments = ["--root", "no-such-directory"];

    [Given("a session in progress")]
    public void GivenASessionInProgress()
    {
        _session = AcceptanceTool.McpSession.Start(Tree.Root);
        _session.Exchange(AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)).Should().NotBeNull();
    }

    [Given("a client asking for the protocol revision the server supports")]
    public void GivenTheSupportedRevision() => _request = AcceptanceTool.McpRequest(1, "tools/list", AcceptanceTool.McpRevision);

    [Given("a client asking for a protocol revision the server does not support")]
    public void GivenAnUnsupportedRevision() => _request = AcceptanceTool.McpRequest(1, "tools/list", UnsupportedRevision);

    [Given("a client that opens with a legacy initialize request")]
    public void GivenALegacyInitialize() => _request = AcceptanceTool.McpLegacyInitialize(1);

    [When("the client sends the protocol's discovery request")]
    public void WhenDiscovery() =>
        _result = AcceptanceTool.LaunchMcp(Tree.Root, [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)]);

    [When("a client starts the server and calls it")]
    public void WhenCalled() => WhenDiscovery();

    [When("the client sends a request")]
    public void WhenARequestIsSent() => _result = AcceptanceTool.LaunchMcp(Tree.Root, [_request]);

    [When("the client sends it")]
    public void WhenItIsSent() => WhenARequestIsSent();

    [When("a session runs from start to end")]
    public void WhenASessionRuns()
    {
        _before = AcceptanceTool.Snapshot(Tree.Root);
        using var session = AcceptanceTool.McpSession.Start(Tree.Root);
        session.Exchange(AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)).Should().NotBeNull();
        if (!OperatingSystem.IsWindows())
        {
            _listening = session.Sockets("-iTCP", "-sTCP:LISTEN");
            _open = session.Sockets("-i", "-sTCP:^LISTEN");
        }

        session.Exchange("this is not a protocol message");
        session.Exchange(AcceptanceTool.McpRequest(2, "tools/list", UnsupportedRevision)).Should().NotBeNull();
        session.Exchange(AcceptanceTool.McpLegacyInitialize(3)).Should().NotBeNull();
        _result = session.Close();
    }

    [When("the client starts the server")]
    public void WhenTheServerStarts() =>
        _result = AcceptanceTool.LaunchMcp(Tree.Root, [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)], _arguments);

    [When("the client closes the server's standard input")]
    public void WhenInputCloses() => _result = _session!.Close();

    [When("help is asked for the MCP")]
    public void WhenHelp() => _result = AcceptanceTool.LaunchMcp(Tree.Root, [], "--help");

    [Then("the server answers with the protocol revisions it supports, its capabilities and its identity")]
    public void ThenIdentity()
    {
        var answer = Answer(1);
        answer["error"].Should().BeNull();
        var result = answer["result"]!.AsObject();
        result["supportedVersions"]!.AsArray().Select(static version => version!.GetValue<string>()).Should().Equal(AcceptanceTool.McpRevision);
        result["capabilities"].Should().BeOfType<JsonObject>();
        var identity = result["_meta"]!["io.modelcontextprotocol/serverInfo"]!.AsObject();
        identity["name"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
        identity["version"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
    }

    [Then("every protocol message passes over the server's standard input and output")]
    public void ThenStandardStreams()
    {
        Answer(1)["result"].Should().NotBeNull();
        _result.Stderr.Split('\n').Should().NotContain(static line => line.Contains("\"jsonrpc\"", StringComparison.Ordinal));
    }

    [Then("the server has opened no listening network port")]
    public void ThenNoListeningPort()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Sockets are read with lsof, which Windows does not have.");
        _listening.Should().BeEmpty();
    }

    [Then("the server has made no outbound network call")]
    public void ThenNoOutboundCall()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Sockets are read with lsof, which Windows does not have.");
        _open.Should().BeEmpty();
    }

    [Then("everything the server wrote on its standard output is a protocol message")]
    public void ThenOnlyProtocolMessages()
    {
        var lines = _result.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        lines.Should().NotBeEmpty();
        foreach (var line in lines)
        {
            var message = JsonNode.Parse(line)!.AsObject();
            message["jsonrpc"]!.GetValue<string>().Should().Be("2.0");
            (message.ContainsKey("result") || message.ContainsKey("error") || message.ContainsKey("method")).Should().BeTrue(line);
        }
    }

    [Then("the version the server states for itself is the version of the MCP's own package")]
    public void ThenOwnVersion()
    {
        var expected = AcceptanceTool.InformationalVersion(File.ReadAllBytes(AcceptanceTool.McpAssemblyPath));
        expected.Should().NotBeNullOrWhiteSpace();
        Answer(1)["result"]!["_meta"]!["io.modelcontextprotocol/serverInfo"]!["version"]!.GetValue<string>().Should().Be(expected);
    }

    [Then("the server has created, modified or deleted no file under the root or elsewhere")]
    public void ThenNoFileChanged() => AcceptanceTool.Snapshot(Tree.Root).Should().BeEquivalentTo(_before);

    [Then("a message naming that root as it was given is written on standard error")]
    public void ThenRootNamed() => _result.Stderr.Should().Contain("no-such-directory").And.NotContain(Tree.Root);

    [Then("the server exits with the missing-input code before any session starts")]
    public void ThenMissingInput()
    {
        _result.ExitCode.Should().Be(MissingInput, _result.Stderr);
        _result.Stdout.Should().BeEmpty();
    }

    [Then("the server answers it")]
    public void ThenAnswered() => Answer(1)["result"].Should().NotBeNull();

    [Then("the server exits with code 0")]
    public void ThenExitsZero() => _result.ExitCode.Should().Be(0, _result.Stderr);

    [Then("the usage names the MCP's options")]
    public void ThenUsage() => (_result.Stdout + _result.Stderr).Should().Contain("--root");

    [Then("the MCP exits with code 0")]
    public void ThenHelpExitsZero() => _result.ExitCode.Should().Be(0, _result.Stderr);

    [Then("the server answers it without a version error")]
    public void ThenNoVersionError() => Answer(1)["error"]?["code"]?.GetValue<int>().Should().NotBe(UnsupportedProtocolVersion);

    [Then("the server answers with the protocol's unsupported-version error")]
    public void ThenUnsupportedVersion() => Answer(1)["error"]!["code"]!.GetValue<int>().Should().Be(UnsupportedProtocolVersion);

    [Then("the error lists the revisions the server supports")]
    public void ThenSupportedListed()
    {
        var listed = Answer(1)["error"]!["data"]!["supported"]!.AsArray().Select(static version => version!.GetValue<string>());
        var (stdout, _, _) = AcceptanceTool.LaunchMcp(Tree.Root, [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)]);
        var supported = AcceptanceTool.McpAnswer(stdout, 1)!["result"]!["supportedVersions"]!.AsArray().Select(static version => version!.GetValue<string>());
        listed.Should().NotBeEmpty().And.Equal(supported);
    }

    [Then("the server answers with an error")]
    public void ThenAnError() => Answer(1)["error"].Should().NotBeNull();

    [Then("the error names the protocol revision the server supports")]
    public void ThenRevisionNamed() => Answer(1)["error"]!.ToJsonString().Should().Contain(AcceptanceTool.McpRevision);

    [AfterScenario]
    public void DisposeSession()
    {
        _session?.Dispose();
        _tree?.Dispose();
    }

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No synthetic root was prepared.");

    private JsonObject Answer(int id) =>
        AcceptanceTool.McpAnswer(_result.Stdout, id) ?? throw new Xunit.Sdk.XunitException(
            $"The server answered request {id} with nothing. Exit {_result.ExitCode}. Stderr: {_result.Stderr}");

    private const int MissingInput = 2;
    private const int UnsupportedProtocolVersion = -32022;
    private const string UnsupportedRevision = "1900-01-01";

    private SpechtTree? _tree;
    private string[] _arguments = [];
    private string _request = string.Empty;
    private IReadOnlyDictionary<string, string> _before = new Dictionary<string, string>();
    private IReadOnlyList<string> _listening = [];
    private IReadOnlyList<string> _open = [];
    private AcceptanceTool.McpSession? _session;
    private (string Stdout, string Stderr, int ExitCode) _result;
}
