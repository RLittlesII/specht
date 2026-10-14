using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Specht.Check;
using Specht.Cqrs;
using Specht.Manifest;
using Specht.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Schema.List;

/// <summary>Lists the catalog and marks the pin, keeping failures off stdout (0001-F7 B-109).</summary>
public sealed class SchemaListCommand(IAnsiConsole console, ICqrs cqrs, SchemaVersions versions, TextWriter? errorWriter = null)
    : AsyncCommand<SchemaListCommand.Settings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var result = await cqrs.Send(new SchemaList.Query(Path.GetFullPath(settings.Root)), cancellationToken);
            foreach (var version in result.Versions)
            {
                console.Profile.Out.Writer.WriteLine($"{version}{(version == result.Pinned ? " (pinned)" : string.Empty)}");
            }
            return ExitCodes.Success;
        }
        catch (SpechtRootNotFoundException)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync("specht: --root must name an existing directory.");
            return ExitCodes.MissingInput;
        }
        catch (Exception exception) when (exception is SpechtManifestException or SpechtManifestUnreadableException)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync(
                $"specht: {exception.Message}\nShipped schema versions: {string.Join(", ", versions.Versions.Select(static version => version.Number))}.\n"
                + "For an unusable pin, run 'specht schema pin <version>'; correct invalid JSON or settings in .specht/manifest.json first.");
            return ExitCodes.InvalidManifest;
        }
    }

    /// <summary>The listing options.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root whose manifest pin is marked.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }
}
