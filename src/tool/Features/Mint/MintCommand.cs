using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Identifiers;
using Specht.Manifest;
using Specht.Model;
using Specht.Tree;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Mint;

/// <summary>
/// Prints one id the engine mints for a type the manifest opts in, and folds a refusal into an exit code the check already
/// gives, with the reason on standard error and nothing on standard output (<c>0001-F19</c> A-2, C-1, C-9, C-10).
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-003" />
public sealed class MintCommand(IAnsiConsole console, SpechtModelReader reader, Minter minter, TextWriter? errors = null)
    : AsyncCommand<MintCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F19" claim="B-001" />
    /// <implements epic="0001" feature="F19" claim="B-002" />
    /// <implements epic="0001" feature="F19" claim="B-003" />
    /// <implements epic="0001" feature="F19" claim="B-004" />
    /// <implements epic="0001" feature="F19" claim="B-015" />
    /// <implements epic="0001" feature="F19" claim="B-016" />
    /// <implements epic="0001" feature="F19" claim="B-017" />
    /// <implements epic="0001" feature="F19" claim="B-043" />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Feature is not null && settings.Type is "adr" or "lesson")
        {
            return await Fail(ExitCodes.MissingInput, $"specht: '{settings.Type}' ids are repository-wide; mint one without --feature.");
        }

        try
        {
            var root = Path.GetFullPath(settings.Root);
            var manifest = reader.ReadManifestDocument(root);
            var id = minter.Mint(manifest.Document, settings.Type, root, manifest.Structure.Discovery.Exclusions);
            await console.Profile.Out.Writer.WriteLineAsync(id);
            return ExitCodes.Success;
        }
        catch (SpechtRootNotFoundException)
        {
            return await Fail(ExitCodes.MissingInput, "specht: --root must name an existing directory.");
        }
        catch (SpechtManifestNotFoundException exception)
        {
            return await Fail(ExitCodes.MissingInput, $"specht: {exception.Message}");
        }
        catch (IdNotOptedInException exception)
        {
            return await Fail(ExitCodes.NotFound, $"specht: {exception.Message}");
        }
        catch (Exception exception) when (exception is SpechtManifestUnreadableException or SpechtManifestException or IdRefusedException)
        {
            return await Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }
    }

    /// <summary>The type, the Feature and the root a request names.</summary>
    /// <implements epic="0001" feature="F19" claim="B-003" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the id type, a member of the manifest's <c>mint</c> key.</summary>
        [CommandArgument(0, "<type>")]
        [Description("The id type, as the manifest's mint key names it.")]
        public string Type { get; init; } = string.Empty;

        /// <summary>Gets the folder of the Feature a per-Feature id is minted for, or nothing.</summary>
        [CommandOption("--feature <DIR>")]
        [Description("The Feature a per-Feature id is minted for. Refused for adr and lesson.")]
        public string? Feature { get; init; }

        /// <summary>Gets the repository root.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }

    /// <implements epic="0001" feature="F19" claim="B-017" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
