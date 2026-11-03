using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Tree;
using Specht.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Schema.List;

/// <summary>
/// Lists the catalog and marks the pin, and the version the check uses where it is not the pin, keeping failures off stdout
/// (0001-F7 B-109, B-148, B-149).
/// </summary>
/// <implements epic="0001" feature="F7" claim="B-001" />
public sealed class SchemaListCommand(IAnsiConsole console, SchemaVersionManager schemas, SchemaVersions versions, TextWriter? errorWriter = null)
    : AsyncCommand<SchemaListCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-001" />
    /// <implements epic="0001" feature="F7" claim="B-148" />
    /// <implements epic="0001" feature="F7" claim="B-149" />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var (Versions, Pinned, InUse) = schemas.List(Path.GetFullPath(settings.Root));
            foreach (var version in Versions)
            {
                var mark = version == Pinned ? " (pinned)" : version == InUse ? " (in use)" : string.Empty;
                console.Profile.Out.Writer.WriteLine($"{version}{mark}");
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
    /// <implements epic="0001" feature="F7" claim="B-001" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root whose manifest pin is marked.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }
}
