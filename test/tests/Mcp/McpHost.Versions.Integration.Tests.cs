using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process and holds what it answers a request naming a revision it does not support, and an initialize
/// request, to (0132-F3 B-019, B-032, B-033, B-036, B-037, B-038, B-039).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostVersionsIntegrationTests
{
    [Theory]
    [ClassData(typeof(UnsupportedRevisionRequests))]
    public void ARequestNamingAnUnsupportedRevision_WhenSent_ShouldBeAnsweredWithTheUnsupportedVersionError(string method, string revision)
    {
        // Given
        const int unsupportedProtocolVersion = -32022;
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpRequest(1, method, revision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["result"].Should().BeNull();
        answer["error"]!["code"]!.GetValue<int>().Should().Be(unsupportedProtocolVersion);
    }

    [Fact]
    public void AnUnsupportedVersionError_WhenAnswered_ShouldListTheRevisionsTheServerSupports()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (refusedStdout, refusedStderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpRequest(1, "tools/list", "1900-01-01")]);
        var (discoveredStdout, discoveredStderr, _) = AcceptanceTool.LaunchMcp(
            tree.Root,
            [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)]);

        // Then
        var refused = AcceptanceTool.McpAnswer(refusedStdout, 1);
        var discovered = AcceptanceTool.McpAnswer(discoveredStdout, 1);
        refused.Should().NotBeNull(refusedStderr);
        discovered.Should().NotBeNull(discoveredStderr);
        var supported = discovered!["result"]!["supportedVersions"]!.AsArray().Select(static version => version!.GetValue<string>()).ToList();
        supported.Should().NotBeEmpty();
        refused!["error"]!["data"]!["supported"]!.AsArray().Select(static version => version!.GetValue<string>()).Should().Equal(supported);
    }

    [Fact]
    public void AnInitializeRequestNamingRevision20260728_WhenSent_ShouldBeAnsweredWithAnErrorNamingTheRevisionAsHavingNoInitializeHandshake()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpInitialize(1, AcceptanceTool.McpRevision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["result"].Should().BeNull();
        answer["error"]!.ToJsonString().Should().Contain(AcceptanceTool.McpRevision).And.Contain("initialize");
    }

    [Theory]
    [ClassData(typeof(HandshakeRevisions))]
    public void AnInitializeRequestNamingAHandshakeRevision_WhenSent_ShouldBeAnsweredWithThatRevisionItsCapabilitiesAndItsIdentity(string revision)
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpInitialize(1, revision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["error"].Should().BeNull();
        var result = answer["result"]!.AsObject();
        result["protocolVersion"]!.GetValue<string>().Should().Be(revision);
        result["capabilities"].Should().BeOfType<System.Text.Json.Nodes.JsonObject>();
        result["serverInfo"]!["name"]!.GetValue<string>().Should().Be("specht-mcp");
    }

    [Theory]
    [ClassData(typeof(EarlierRevisions))]
    public void AnInitializeRequestNamingARevisionEarlierThanTheOldest_WhenSent_ShouldBeAnsweredWithTheNewestHandshakeRevision(string revision)
    {
        // Given
        const string newestHandshakeRevision = "2025-11-25";
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpInitialize(1, revision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["error"].Should().BeNull();
        answer["result"]!["protocolVersion"]!.GetValue<string>().Should().Be(newestHandshakeRevision);
    }

    [Fact]
    public void AnInitializeRequestNamingARevisionLaterThan20260728_WhenSent_ShouldBeAnsweredWithAnErrorNamingTheRevisionAsHavingNoInitializeHandshake()
    {
        // Given
        const string laterRevision = "2099-01-01";
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpInitialize(1, laterRevision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["result"].Should().BeNull();
        answer["error"]!.ToJsonString().Should().Contain(laterRevision).And.Contain("initialize");
    }
}
