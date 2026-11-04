using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds that the built MCP opens no port and makes no outbound call: its sockets while it serves, and the networking
/// assemblies its code references (0132-F3 B-003, B-004).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostNetworkIntegrationTests
{
    [Fact]
    public void ASessionThatHasServed_WhenItsSocketsAreListed_ShouldHoldNoListeningSocket()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Sockets are read with lsof, which Windows does not have.");
        using var tree = new SpechtTree();
        using var session = ServedSession(tree);

        // When
        var listening = session.Sockets("-iTCP", "-sTCP:LISTEN");

        // Then
        listening.Should().BeEmpty();
    }

    [Fact]
    public void ASessionThatHasServed_WhenItsSocketsAreListed_ShouldHoldNoConnectionAndNoDatagramSocket()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), "Sockets are read with lsof, which Windows does not have.");
        using var tree = new SpechtTree();
        using var session = ServedSession(tree);

        // When
        var open = session.Sockets("-i", "-sTCP:^LISTEN");

        // Then
        open.Should().BeEmpty();
    }

    [Fact]
    public void TheMcpAssembly_WhenItsReferencesAreRead_ShouldNameNoNetworkingAssembly()
    {
        // Given
        using var file = new PEReader(File.OpenRead(AcceptanceTool.McpAssemblyPath));
        var reader = file.GetMetadataReader();

        // When
        var references = reader.AssemblyReferences
            .Select(handle => reader.GetString(reader.GetAssemblyReference(handle).Name))
            .ToList();

        // Then
        references.Where(name => NetworkAssemblies.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal))).Should().BeEmpty();
    }

    private static AcceptanceTool.McpSession ServedSession(SpechtTree tree)
    {
        var session = AcceptanceTool.McpSession.Start(tree.Root);
        try
        {
            session.Send(AcceptanceTool.McpRequest(1, "server/discover", AcceptanceTool.McpRevision));
            session.ReadLine().Should().NotBeNull("the session must have served before its sockets are read");

            return session;
        }
        catch
        {
            session.Dispose();
            throw;
        }
    }

    private static readonly string[] NetworkAssemblies =
    [
        "System.Net.Sockets",
        "System.Net.Http",
        "System.Net.HttpListener",
        "System.Net.WebSockets",
        "System.Net.Security",
        "System.Net.NameResolution",
        "System.Net.Requests",
        "System.Net.WebClient",
        "System.Net.Ping",
        "Microsoft.AspNetCore",
    ];
}
