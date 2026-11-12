using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp;

namespace Specht.Tests.Mcp;

/// <summary>
/// The tools the host registers through the factory's composition, found by name and called as the protocol calls them, over a scripted set of
/// programs and no process: the tool a name resolves to is whatever the registrations hold, so a test of a tool that is not registered
/// fails on its absence and not on a missing type.
/// </summary>
internal sealed class RegisteredTools : IDisposable
{
    public RegisteredTools(ScriptedProcessRun process)
    {
        _provider = HostFactory.Compose(process).BuildServiceProvider();
        _server = McpServer.Create(new StreamServerTransport(Stream.Null, Stream.Null), new McpServerOptions());
    }

    public IReadOnlyList<string> Names => _provider.GetServices<McpServerTool>().Select(static tool => tool.ProtocolTool.Name).ToList();

    public ModelContextProtocol.Protocol.Tool Declared(string name) => Find(name).ProtocolTool;

    public Task<CallToolResult> CallAsync(string name, string? argument = null, object? value = null) =>
        CallAsync(name, argument is null ? new Dictionary<string, object?>() : new Dictionary<string, object?> { [argument] = value });

    public async Task<CallToolResult> CallAsync(string name, IReadOnlyDictionary<string, object?> values)
    {
        var arguments = values.ToDictionary(static pair => pair.Key, static pair => JsonSerializer.SerializeToElement(pair.Value));
        var parameters = new CallToolRequestParams { Name = name, Arguments = arguments };
        var request = new JsonRpcRequest { Id = new RequestId(1), Method = RequestMethods.ToolsCall };

        return await Find(name).InvokeAsync(new RequestContext<CallToolRequestParams>(_server, request, parameters), CancellationToken.None);
    }

    public void Dispose()
    {
        _provider.Dispose();
        _server.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private McpServerTool Find(string name) =>
        _provider.GetServices<McpServerTool>().SingleOrDefault(tool => tool.ProtocolTool.Name == name)
        ?? throw new Xunit.Sdk.XunitException($"The host registers no '{name}' tool; it registers {string.Join(", ", Names)}.");

    private readonly ServiceProvider _provider;

    private readonly McpServer _server;
}
