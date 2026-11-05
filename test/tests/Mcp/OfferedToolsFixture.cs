using System.Text.Json.Nodes;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>Reads the tools the built MCP lists for a root, and fails when it does not list any.</summary>
internal sealed class OfferedToolsFixture : AutoFixtureBase<OfferedToolsFixture>
{
    public OfferedToolsFixture WithRoot(string root) => With(ref _root, root);

    public static implicit operator List<JsonObject>(OfferedToolsFixture fixture)
    {
        using var session = AcceptanceTool.McpSession.Start(fixture._root);
        var listing = session.Exchange(AcceptanceTool.McpRequest(10, "tools/list", AcceptanceTool.McpRevision));
        session.Close();

        return (JsonNode.Parse(listing ?? "null")?["result"]?["tools"] as JsonArray
            ?? throw new Xunit.Sdk.XunitException($"The host did not list its tools; tools/list answered: {listing ?? "nothing"}"))
            .Select(static tool => tool!.AsObject())
            .ToList();
    }

    private string _root = ".";
}
