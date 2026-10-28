using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Manifest;

/// <summary>Prints the manifest the check reads as plain JSON.</summary>
/// <implements epic="0001" feature="F9" claim="B-001" />
public sealed class ManifestCommand(IAnsiConsole console, EffectiveManifestReader manifests, TextWriter? errors = null)
    : AsyncCommand<ManifestCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F9" claim="B-001" />
    /// <implements epic="0001" feature="F9" claim="B-002" />
    /// <implements epic="0001" feature="F9" claim="B-008" />
    /// <implements epic="0001" feature="F9" claim="B-009" />
    /// <implements epic="0001" feature="F9" claim="B-010" />
    /// <implements epic="0001" feature="F9" claim="B-011" />
    /// <implements epic="0001" feature="F9" claim="B-013" />
    /// <implements epic="0001" feature="F9" claim="B-014" />
    /// <implements epic="0001" feature="F9" claim="B-015" />
    /// <implements epic="0001" feature="F9" claim="B-016" />
    /// <implements epic="0001" feature="F9" claim="B-017" />
    /// <implements epic="0001" feature="F9" claim="B-018" />
    /// <implements epic="0001" feature="F9" claim="B-019" />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var text = settings.Defaults
                ? manifests.ReadDefaultText(!settings.Json)
                : manifests.ReadText(Path.GetFullPath(settings.Root), !settings.Json);
            await console.Profile.Out.Writer.WriteLineAsync(text);
            return ExitCodes.Success;
        }
        catch (SpechtRootNotFoundException)
        {
            return await Fail(ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory.");
        }
        catch (SpechtManifestNotFoundException exception)
        {
            return await Fail(ExitCodes.MissingInput, $"specht: {exception.Message}");
        }
        catch (Exception exception) when (exception is SpechtManifestUnreadableException or SpechtManifestException)
        {
            return await Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }
    }

    /// <summary>The manifest inspection options.</summary>
    /// <implements epic="0001" feature="F9" claim="B-001" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root whose effective manifest is printed.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Gets whether the newest shipped default is printed without reading a root.</summary>
        [CommandOption("--defaults")]
        [Description("Print the newest shipped default without reading a root.")]
        public bool Defaults { get; init; }

        /// <summary>Gets whether the selected manifest is printed as one compact JSON document.</summary>
        [CommandOption("--json")]
        [Description("Print the manifest as one JSON document.")]
        public bool Json { get; init; }
    }

    /// <implements epic="0001" feature="F9" claim="B-013" />
    /// <implements epic="0001" feature="F9" claim="B-014" />
    /// <implements epic="0001" feature="F9" claim="B-015" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
