using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Versioning;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Schema.Pin;

/// <summary>Pins the caller's selected schema version and maps input failures to their CLI exit codes.</summary>
public sealed class SchemaPinCommand(IAnsiConsole console, SchemaVersionManager schemas, TextWriter? errorWriter = null)
    : AsyncCommand<SchemaPinCommand.Settings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var root = Path.GetFullPath(settings.Root);
            var version = settings.Version;
            if (version is null)
            {
                var choices = schemas.Choices(root);
                if (!console.Profile.Capabilities.Interactive)
                {
                    await (errorWriter ?? Console.Error).WriteLineAsync(
                        "specht: selecting a version requires an interactive terminal; use specht schema pin <version>.");
                    return ExitCodes.MissingInput;
                }
                version = console.Prompt(new SelectionPrompt<SemanticVersion>()
                    .Title("Select schema version")
                    .AddChoices(choices)).ToString();
            }
            var selected = await schemas.Pin(root, version, cancellationToken);
            console.Profile.Out.Writer.WriteLine($"pinned schemaVersion {selected} in {SpecManifestStore.RelativePath}");
            return ExitCodes.Success;
        }
        catch (SpechtRootNotFoundException)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync("specht: --root must name an existing directory.");
            return ExitCodes.MissingInput;
        }
        catch (SpechtManifestNotFoundException exception)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync($"specht: {exception.Message}");
            return ExitCodes.MissingInput;
        }
        catch (SchemaVersionNotFoundException exception)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync($"specht: {exception.Message}");
            return ExitCodes.NotFound;
        }
        catch (Exception exception) when (exception is SpechtManifestException or SpechtManifestUnreadableException)
        {
            await (errorWriter ?? Console.Error).WriteLineAsync($"specht: {exception.Message}");
            return ExitCodes.InvalidManifest;
        }
    }

    /// <summary>The pin options.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the optional target version.</summary>
        [CommandArgument(0, "[version]")]
        [Description("The shipped schema version to pin; omit to select interactively.")]
        public string? Version { get; init; }

        /// <summary>Gets the repository root.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }
}
