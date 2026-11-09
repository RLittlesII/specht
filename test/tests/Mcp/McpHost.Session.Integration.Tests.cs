using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process and holds the session around its answers: the streams, the exit codes, the files and the
/// options (0132-F3 B-002, B-005, B-008, B-010, B-012, B-013).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostSessionIntegrationTests
{
    [Fact]
    public void TwoRequests_WhenSentOnStandardInput_ShouldBeAnsweredOnStandardOutputAndNotOnStandardError()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(
            tree.Root,
            [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision), AcceptanceTool.McpRequest(2, "tools/list", "1900-01-01")]);

        // Then
        AcceptanceTool.McpAnswer(stdout, 1).Should().NotBeNull(stderr);
        AcceptanceTool.McpAnswer(stdout, 2).Should().NotBeNull(stderr);
        stderr.Split('\n').Should().NotContain(static line => line.Contains("\"jsonrpc\"", StringComparison.Ordinal));
    }

    [Fact]
    public void ASessionThatIsGivenRequestsAndNoise_WhenItRuns_ShouldWriteOnlyProtocolMessagesOnStandardOutput()
    {
        // Given
        using var tree = new SpechtTree();
        string[] input =
        [
            AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision),
            "this is not a protocol message",
            AcceptanceTool.McpRequest(2, "tools/list", "1900-01-01"),
            AcceptanceTool.McpInitialize(3, "2025-11-25"),
        ];

        // When
        var (stdout, _, _) = AcceptanceTool.LaunchMcp(tree.Root, input);

        // Then
        var lines = stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        lines.Should().NotBeEmpty();
        lines.Where(static line => !IsProtocolMessage(line)).Should().BeEmpty();

        static bool IsProtocolMessage(string line)
        {
            try
            {
                return JsonNode.Parse(line) is JsonObject message
                    && message["jsonrpc"]?.GetValue<string>() == "2.0"
                    && (message.ContainsKey("result") || message.ContainsKey("error") || message.ContainsKey("method"));
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }

    [Fact]
    public void ASession_WhenItRunsFromStartToEnd_ShouldWriteNoFileUnderTheRoot()
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw("sentinel.bin", "unchanged");
        var before = AcceptanceTool.Snapshot(tree.Root);

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(
            tree.Root,
            [
                AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision),
                AcceptanceTool.McpRequest(2, "tools/list", "1900-01-01"),
                AcceptanceTool.McpInitialize(3, "2025-11-25"),
            ]);

        // Then
        AcceptanceTool.McpAnswer(stdout, 1).Should().NotBeNull(stderr);
        AcceptanceTool.Snapshot(tree.Root).Should().BeEquivalentTo(before);
    }

    [Theory]
    [ClassData(typeof(NonDirectoryRoots))]
    public void ARootThatIsNotADirectory_WhenTheServerStarts_ShouldNameItOnStandardErrorAndExitBeforeAnySession(string root, bool exists)
    {
        // Given
        const int missingInput = 2;
        using var tree = new SpechtTree();
        if (exists)
        {
            tree.WriteRaw(root, "not a directory");
        }

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.LaunchMcp(
            tree.Root,
            [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)],
            "--root",
            root);

        // Then
        exitCode.Should().Be(missingInput, stderr);
        stderr.Should().Contain(root).And.NotContain(tree.Root);
        stdout.Should().BeEmpty();
    }

    [Fact]
    public void ASessionInProgress_WhenTheClientClosesStandardInput_ShouldExitWithCodeZero()
    {
        // Given
        using var tree = new SpechtTree();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root);
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.Exchange(AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)).Should().NotBeNull();

        // When
        var (_, stderr, exitCode) = session.Close();

        // Then
        exitCode.Should().Be(0, stderr);
    }

    [Fact]
    public void HelpAsked_WhenTheServerStarts_ShouldNameTheRootOptionAndExitWithCodeZero()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.LaunchMcp(tree.Root, [], "--help");

        // Then
        exitCode.Should().Be(0, stderr);
        (stdout + stderr).Should().Contain("--root");
    }
}
