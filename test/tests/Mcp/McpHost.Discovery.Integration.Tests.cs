using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process and holds what it answers a discovery request and a request of the supported revision to
/// (0132-F3 B-001, B-006, B-011, B-014).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostDiscoveryIntegrationTests
{
    [Fact]
    public void ADiscoveryRequest_WhenServed_ShouldAnswerWithTheSupportedRevisionsItsCapabilitiesAndItsIdentity()
    {
        // Given
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["error"].Should().BeNull();
        var result = answer["result"]!.AsObject();
        result["supportedVersions"]!.AsArray().Select(static version => version!.GetValue<string>()).Should().Equal(AcceptanceTool.McpRevision);
        result["capabilities"].Should().BeOfType<System.Text.Json.Nodes.JsonObject>();
        var identity = result["_meta"]!["io.modelcontextprotocol/serverInfo"]!.AsObject();
        identity["name"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
        identity["version"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ADiscoveryAnswer_WhenItStatesTheServerVersion_ShouldStateTheInformationalVersionOfTheMcpAssembly()
    {
        // Given
        using var tree = new SpechtTree();
        var expected = AcceptanceTool.InformationalVersion(File.ReadAllBytes(AcceptanceTool.McpAssemblyPath));

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)]);

        // Then
        expected.Should().NotBeNullOrWhiteSpace();
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["result"]!["_meta"]!["io.modelcontextprotocol/serverInfo"]!["version"]!.GetValue<string>().Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(MissingOrRejectedManifests))]
    public void ARootWithoutAUsableManifest_WhenAskedForDiscovery_ShouldAnswerIt(bool removed, string manifest)
    {
        // Given
        using var tree = new SpechtTree();
        if (removed)
        {
            File.Delete(Path.Combine(tree.Root, SpechtTree.ManifestPath));
        }
        else
        {
            tree.WriteRaw(SpechtTree.ManifestPath, manifest);
        }

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["error"].Should().BeNull();
        answer["result"].Should().NotBeNull();
    }

    [Fact]
    public void ARequestNamingTheSupportedRevision_WhenSent_ShouldNotBeAnsweredWithAVersionError()
    {
        // Given
        const int unsupportedProtocolVersion = -32022;
        using var tree = new SpechtTree();

        // When
        var (stdout, stderr, _) = AcceptanceTool.LaunchMcp(tree.Root, [AcceptanceTool.McpRequest(1, "tools/list", AcceptanceTool.McpRevision)]);

        // Then
        var answer = AcceptanceTool.McpAnswer(stdout, 1);
        answer.Should().NotBeNull(stderr);
        answer!["error"]?["code"]?.GetValue<int>().Should().NotBe(unsupportedProtocolVersion);
    }

    [Theory]
    [ClassData(typeof(HandshakeRevisions))]
    public void ARequestInASessionOpenedAtAHandshakeRevision_WhenSent_ShouldNotBeAnsweredWithAVersionError(string revision)
    {
        // Given
        const int unsupportedProtocolVersion = -32022;
        using var tree = new SpechtTree();
        using var session = AcceptanceTool.McpSession.Start(tree.Root);
        session.Exchange(AcceptanceTool.McpInitialize(1, revision)).Should().NotBeNull();
        session.Send(AcceptanceTool.McpInitialized());

        // When
        var line = session.Exchange(AcceptanceTool.McpSessionRequest(2, "tools/list"));

        // Then
        line.Should().NotBeNull();
        var answer = AcceptanceTool.McpAnswer(line!, 2);
        answer.Should().NotBeNull(line);
        answer!["error"]?["code"]?.GetValue<int>().Should().NotBe(unsupportedProtocolVersion);
    }
}
