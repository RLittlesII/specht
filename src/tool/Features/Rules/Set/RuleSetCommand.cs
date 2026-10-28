using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Rules.Set;

/// <summary>Sets one rule level and maps engine input failures to the existing exit codes.</summary>
/// <implements epic="0001" feature="F11" claim="B-002" />
public sealed class RuleSetCommand(RuleLevelSetter setter, TextWriter? errors = null) : AsyncCommand<RuleSetCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F11" claim="B-002" />
    /// <implements epic="0001" feature="F11" claim="B-007" />
    /// <implements epic="0001" feature="F11" claim="B-008" />
    /// <implements epic="0001" feature="F11" claim="B-009" />
    /// <implements epic="0001" feature="F11" claim="B-010" />
    /// <implements epic="0001" feature="F11" claim="B-011" />
    /// <implements epic="0001" feature="F11" claim="B-013" />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            await setter.Set(Path.GetFullPath(settings.Root), settings.Id, settings.Level, cancellationToken);
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
        catch (RuleLevelNotFoundException exception)
        {
            return await Fail(ExitCodes.NotFound, $"specht: {exception.Message}");
        }
        catch (Exception exception) when (exception is SpechtManifestUnreadableException or SpechtManifestException)
        {
            return await Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }
    }

    /// <summary>The rule id, level and inherited root options.</summary>
    /// <implements epic="0001" feature="F11" claim="B-001" />
    public sealed class Settings : RulesCommand.BranchSettings
    {
        /// <summary>Gets the exact rule id in the pinned vocabulary.</summary>
        [CommandArgument(0, "<id>")]
        [Description("The rule id from the pinned schema version.")]
        public string Id { get; init; } = string.Empty;

        /// <summary>Gets the exact lower-case rule level.</summary>
        [CommandArgument(1, "<level>")]
        [Description("The rule level: error, warning or off.")]
        public string Level { get; init; } = string.Empty;
    }

    /// <implements epic="0001" feature="F11" claim="B-013" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
