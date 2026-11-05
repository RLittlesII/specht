using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process and holds what it answers a request naming a revision it does not support, and a legacy
/// initialize, to (0132-F3 B-019, B-032, B-033).
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
    public void ALegacyInitializeRequest_WhenSent_ShouldBeAnsweredWithAnErrorNamingTheSupportedRevision()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpLegacyInitialize(1)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["result"].Should().BeNull();
        answer["error"].Should().NotBeNull();
        answer["error"]!.ToJsonString().Should().Contain(AcceptanceTool.McpRevision);
    }
}
