using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Spectre.Console.Cli;

namespace Specht.Mcp.Features.Host;

/// <summary>
/// <c>specht-mcp</c>, the one command (<c>0132-F3</c>): refuses a root that is not a directory, then serves the protocol over
/// standard input and output until the client closes it. The protocol is the SDK's; this command names the revision it
/// supports, its identity and its capabilities, and defines no answer (C-5).
/// </summary>
/// <implements epic="0132" feature="F3" claim="B-002" />
public sealed class HostCommand : AsyncCommand<HostCommand.Settings>
{
    /// <summary>The server's name, and the application's.</summary>
    public const string Name = "specht-mcp";

    /// <summary>The one protocol revision the host supports (<c>0132-F3</c> OQ-5).</summary>
    public const string Revision = "2026-07-28";

    /// <inheritdoc />
    /// <implements epic="0132" feature="F3" claim="B-002" />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(settings.Root))
        {
            await Console.Error.WriteLineAsync($"{Name}: '{settings.Root}' is not a directory.");

            return MissingInput;
        }

        var options = new McpServerOptions
        {
            ProtocolVersion = Revision,
            ServerInfo = new Implementation { Name = Name, Version = ThisAssembly.AssemblyInformationalVersion },
            Capabilities = new ServerCapabilities(),
        };

        await using var server = McpServer.Create(new StdioServerTransport(Name), options);
        await server.RunAsync(cancellationToken);

        return 0;
    }

    /// <summary>The host's options.</summary>
    /// <implements epic="0132" feature="F3" claim="B-013" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the repository root to answer for, as typed; the working directory when omitted.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root to answer for. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }

    private const int MissingInput = 2;
}
