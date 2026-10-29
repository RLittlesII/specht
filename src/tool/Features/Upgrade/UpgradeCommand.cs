using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Upgrade;

/// <summary>
/// <c>specht upgrade</c> (<c>0001-F7</c>): brings the root's present frontmatter schemas and templates to the pinned version,
/// adds the manifest keys that version defines, and names the version and each rewritten file, or says the root is already
/// current. It parses, calls the writer, prints, and folds - nothing else.
/// </summary>
/// <param name="console">Where the current notice and the listing go.</param>
/// <param name="writer">The comparison and rewrite.</param>
/// <param name="errorWriter">Where failures go; standard error when omitted.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
public sealed class UpgradeCommand(IAnsiConsole console, UpgradeWriter writer, TextWriter? errorWriter = null)
    : AsyncCommand<UpgradeCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F7" claim="B-005" />
    /// <implements epic="0001" feature="F7" claim="B-017" />
    /// <implements epic="0001" feature="F7" claim="B-018" />
    /// <implements epic="0001" feature="F7" claim="B-019" />
    /// <implements epic="0001" feature="F7" claim="B-020" />
    /// <implements epic="0001" feature="F7" claim="B-059" />
    /// <implements epic="0001" feature="F7" claim="B-027" />
    /// <implements epic="0001" feature="F7" claim="B-035" />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var result = await writer.Upgrade(Path.GetFullPath(settings.Root), cancellationToken);

            if (result.Rewritten.Count == 0)
            {
                console.Profile.Out.Writer.WriteLine($"{SpecManifestStore.ConfigurationFolder}/ is current at schemaVersion {result.Pinned}");
            }
            else
            {
                console.Profile.Out.Writer.WriteLine($"{SpecManifestStore.ConfigurationFolder}/ rewritten to schemaVersion {result.Pinned}");
                foreach (var path in result.Rewritten)
                {
                    console.Profile.Out.Writer.WriteLine($"rewritten {path}");
                }
            }

            return ExitCodes.Success;
        }
        catch (Exception exception) when (exception is SpechtRootNotFoundException or SpechtManifestNotFoundException)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync($"specht: {exception.Message}");

            return ExitCodes.MissingInput;
        }
        catch (Exception exception) when (exception is SpechtManifestException or SpechtManifestUnreadableException or SpechtUpstreamSchemaException)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync($"specht: {exception.Message}");

            return ExitCodes.InvalidManifest;
        }
    }

    /// <summary>The upgrade command's options.</summary>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the repository root, as typed; the working directory when omitted.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root to upgrade. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }
}
