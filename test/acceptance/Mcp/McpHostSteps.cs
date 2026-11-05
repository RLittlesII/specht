using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Mcp;

/// <summary>
/// Runs the built MCP as a process and holds what the host does over its standard input and output (0132-F3 B-001 to B-006,
/// B-008, B-010 to B-014, B-019, B-032, B-033) and the pinned-tool resolution through the tool check (0359: B-017, B-018, B-020 to
/// B-030, B-034), whose scenarios stay red until 0330 registers it. The documentation scenarios (0332) have no binding here, and
/// B-009's wait for the first answer the tool serves (0330).
/// </summary>
[Binding]
[Scope(Feature = "The MCP host")]
public sealed class McpHostSteps
{
    [Given("a repository whose local tool manifest lists the tool")]
    public void GivenAToolManifest()
    {
        _tree = new SpechtTree();
        _tools = new AcceptanceTool.FakeTools();
        _tools.Pin(Tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: false);
    }

    [Given("the pinned tool is restored")]
    public void GivenTheToolIsRestored()
    {
        if (!OperatingSystem.IsWindows())
        {
            _tools!.Pin(Tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        }
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

    [Given("the repository pins the tool at a version other than the MCP's own")]
    public void GivenAPinOtherThanTheMcps()
    {
        Fakes();
        AcceptanceTool.FakeTools.PinnedVersion.Should().NotBe(AcceptanceTool.InformationalVersion(File.ReadAllBytes(AcceptanceTool.McpAssemblyPath)));
    }

    [Given("another version of the tool is reachable from the path")]
    public void GivenAToolOnThePath() => Fakes().InstallOnPath(AcceptanceTool.FakeTools.PathVersion);

    [Given("the pinned tool's restored copy is removed")]
    public void GivenTheRestoredCopyIsRemoved() => Fakes().Unrestore();

    [Given("a global tool is installed")]
    public void GivenAGlobalTool() => Fakes().InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);

    [Given("no global tool is installed")]
    public void GivenNoGlobalTool() => Fakes();

    [Given("a global tool is installed that is older than the minimum version the server requires")]
    public void GivenAnOldGlobalTool() => Fakes().InstallGlobal(AcceptanceTool.FakeTools.OldVersion);

    [Given(@"^the repository's local tool manifest (is removed|no longer lists the tool)$")]
    public void GivenNoPin(string state) => Fakes().Unpin(Tree.Root, removed: state == "is removed");

    [Given("the tool is older than the minimum version the server requires")]
    public void GivenAnOldPinnedTool() => Fakes().Pin(Tree.Root, AcceptanceTool.FakeTools.OldVersion, restored: true);

    [Given("a session in which a client calls the check tool twice")]
    public void GivenTwoCalls() => _calls = 2;

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

    [When("^(?:a|the) client calls the check tool$")]
    public void WhenCheckIsCalled() => RunCalls(Fakes());

    [When("the session ends")]
    public void WhenTheSessionEnds() => RunCalls(Fakes());

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

    [Then("the tool that ran is the pinned version")]
    public void ThenThePinnedRan() =>
        Work().Should().NotBeEmpty().And.OnlyContain(static run =>
            run.Label == AcceptanceTool.FakeTools.Pinned && run.Version == AcceptanceTool.FakeTools.PinnedVersion);

    [Then("the tool that ran is the pinned version and not the other")]
    public void ThenOnlyThePinnedRan()
    {
        Fakes().Runs.Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Pinned);
    }

    [Then("the server has not restored the pinned tool")]
    public void ThenNothingRestored()
    {
        Fakes().AnythingRestored.Should().BeFalse();
        AcceptanceTool.Snapshot(Tree.Root).Should().BeEquivalentTo(_before);
    }

    [Then("the call fails with an error")]
    public void ThenTheCallFails() => AcceptanceTool.McpFailure(_answers.First()).Should().NotBeNull();

    [Then("the error names the tool and the version that is missing")]
    public void ThenTheMissingToolIsNamed() =>
        Failure().Should().Contain("specht.tool").And.Contain(AcceptanceTool.FakeTools.PinnedVersion);

    [Then("the error names where to read how to restore the tool")]
    public void ThenRestoreIsPointedTo() => Failure().Should().Contain("docs/mcp.md#restore-the-tool");

    [Then("no other tool ran")]
    public void ThenNoToolRan() => Fakes().Runs.Should().BeEmpty();

    [Then("the global tool ran")]
    public void ThenTheGlobalRan() =>
        Work().Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Global);

    [Then("the result carries a part, apart from the part holding the command's output, that tells the user the answer is unpinned")]
    public void ThenAnUnpinnedPart()
    {
        var blocks = AcceptanceTool.McpTextBlocks(_answers.First());
        blocks.Where(static block => block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal)).Should().ContainSingle();
        UnpinnedReports().Should().ContainSingle();
    }

    [Then("the report names where to read how to pin the tool")]
    public void ThenPinIsPointedTo() => UnpinnedReports().Should().ContainSingle().Which.Should().Contain("docs/mcp.md#pin-the-tool");

    [Then("the server has checked the tool's version once")]
    public void ThenCheckedOnce()
    {
        _answers.Select(AcceptanceTool.McpFailure).Should().AllSatisfy(static failure => failure.Should().BeNull());
        Fakes().Runs.Count(static run => run.IsVersionCheck).Should().Be(1);
    }

    [Then("the server has checked the global tool's version once")]
    public void ThenGlobalCheckedOnce()
    {
        ThenCheckedOnce();
        Fakes().Runs.Where(static run => run.IsVersionCheck).Should().ContainSingle().Which.Label.Should().Be(AcceptanceTool.FakeTools.Global);
    }

    [Then("the tool has answered nothing")]
    public void ThenTheToolWasGivenNoWork() => Work().Should().BeEmpty();

    [Then("the global tool has answered nothing")]
    public void ThenTheGlobalToolWasGivenNoWork() => Work().Should().BeEmpty();

    [Then("the error names the minimum version required")]
    public void ThenTheMinimumIsNamed() =>
        Regex.Matches(Failure(), @"\d+\.\d+\.\d+")
            .Select(static match => match.Value)
            .Should()
            .Contain(static version => version != AcceptanceTool.FakeTools.OldVersion);

    [Then("the error names where to read how to update the tool")]
    public void ThenUpdateIsPointedTo() => Failure().Should().Contain("docs/mcp.md#update-the-tool");

    [Then("the error names the pin and the global tool as not found")]
    public void ThenNeitherIsFound() => Failure().Should().ContainEquivalentOf("pin").And.ContainEquivalentOf("global").And.ContainEquivalentOf("not found");

    [AfterScenario]
    public void DisposeSession()
    {
        _session?.Dispose();
        _tools?.Dispose();
        _tree?.Dispose();
    }

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No synthetic root was prepared.");

    private AcceptanceTool.FakeTools Fakes()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The fake tools are shell scripts, which Windows does not run.");

        return _tools ?? throw new InvalidOperationException("No fake tools were prepared.");
    }

    private void RunCalls(AcceptanceTool.FakeTools tools)
    {
        _before = AcceptanceTool.Snapshot(Tree.Root);
        using var session = AcceptanceTool.McpSession.Start(Tree.Root, tools.Environment);
        _answers = Enumerable.Range(0, _calls).Select(index => session.CallTool(10 + (index * 10), "check")).ToList();
        _result = session.Close();
    }

    private IReadOnlyList<AcceptanceTool.FakeRun> Work() => Fakes().Runs.Where(static run => !run.IsVersionCheck).ToList();

    private string Failure() => AcceptanceTool.McpFailure(_answers.First()) ?? throw new Xunit.Sdk.XunitException("The call did not fail.");

    private IReadOnlyList<string> UnpinnedReports() =>
        AcceptanceTool.McpTextBlocks(_answers.First())
            .Where(static block => !block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Where(static block => block.Contains("unpinned", StringComparison.OrdinalIgnoreCase))
            .ToList();

    private JsonObject Answer(int id) =>
        AcceptanceTool.McpAnswer(_result.Stdout, id) ?? throw new Xunit.Sdk.XunitException(
            $"The server answered request {id} with nothing. Exit {_result.ExitCode}. Stderr: {_result.Stderr}");

    private const int MissingInput = 2;
    private const int UnsupportedProtocolVersion = -32022;
    private const string UnsupportedRevision = "1900-01-01";

    private SpechtTree? _tree;
    private AcceptanceTool.FakeTools? _tools;
    private int _calls = 1;
    private IReadOnlyList<JsonObject> _answers = [];
    private string[] _arguments = [];
    private string _request = string.Empty;
    private IReadOnlyDictionary<string, string> _before = new Dictionary<string, string>();
    private IReadOnlyList<string> _listening = [];
    private IReadOnlyList<string> _open = [];
    private AcceptanceTool.McpSession? _session;
    private (string Stdout, string Stderr, int ExitCode) _result;
}
