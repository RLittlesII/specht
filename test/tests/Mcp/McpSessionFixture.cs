using AwesomeAssertions;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Acceptance;

namespace Specht.Tests.Mcp;

/// <summary>
/// Starts the built MCP in a root, over the environment of fake tools when a test supplies them, and, when asked, only after it has
/// served a discovery request.
/// </summary>
internal sealed class McpSessionFixture : AutoFixtureBase<McpSessionFixture>
{
    public McpSessionFixture WithRoot(string root) => With(ref _root, root);

    public McpSessionFixture WithTools(AcceptanceTool.FakeTools tools) => With(ref _tools, tools);

    public McpSessionFixture WithArguments(params string[] arguments) => With(ref _arguments, arguments);

    public McpSessionFixture WithDiscoveryServed() => With(ref _discoveryServed, true);

    public static implicit operator AcceptanceTool.McpSession(McpSessionFixture fixture)
    {
        var session = fixture._tools is null
            ? AcceptanceTool.McpSession.Start(fixture._root, fixture._arguments)
            : AcceptanceTool.McpSession.Start(fixture._root, fixture._tools.Environment, fixture._arguments);
        if (!fixture._discoveryServed)
        {
            return session;
        }

        try
        {
            session.Exchange(AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision))
                .Should().NotBeNull("the session must have served before its sockets are read");

            return session;
        }
        catch
        {
            session.Dispose();
            throw;
        }
    }

    private string _root = ".";

    private AcceptanceTool.FakeTools? _tools;

    private string[] _arguments = [];

    private bool _discoveryServed;
}
