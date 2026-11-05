using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>Arranges how the built MCP starts: in a root, over the environment of fake tools when a test supplies them, with arguments.</summary>
internal sealed class McpSessionFixture : AutoFixtureBase<McpSessionFixture>
{
    public McpSessionFixture WithRoot(string root) => With(ref _root, root);

    public McpSessionFixture WithTools(AcceptanceTool.FakeTools tools) => With(ref _tools, tools);

    public McpSessionFixture WithArguments(params string[] arguments) => With(ref _arguments, arguments);

    public static implicit operator McpSessionStart(McpSessionFixture fixture) =>
        new(fixture._root, fixture._tools?.Environment ?? new Dictionary<string, string>(StringComparer.Ordinal), fixture._arguments);

    private string _root = ".";

    private AcceptanceTool.FakeTools? _tools;

    private string[] _arguments = [];
}
