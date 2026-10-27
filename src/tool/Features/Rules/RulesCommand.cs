using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Rules;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Rules;

/// <summary>Lists the pinned rule catalogue as plain text or JSON.</summary>
/// <implements epic="0001" feature="F10" claim="B-001" />
public sealed class RulesCommand(IAnsiConsole console, RuleEntriesReader entries, TextWriter? errors = null)
    : AsyncCommand<RulesCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F10" claim="B-001" />
    /// <implements epic="0001" feature="F10" claim="B-009" />
    /// <implements epic="0001" feature="F10" claim="B-010" />
    /// <implements epic="0001" feature="F10" claim="B-011" />
    /// <implements epic="0001" feature="F10" claim="B-012" />
    /// <implements epic="0001" feature="F10" claim="B-013" />
    /// <implements epic="0001" feature="F10" claim="B-014" />
    /// <implements epic="0001" feature="F10" claim="B-015" />
    /// <implements epic="0001" feature="F10" claim="B-016" />
    /// <implements epic="0001" feature="F10" claim="B-017" />
    /// <implements epic="0001" feature="F10" claim="B-018" />
    /// <implements epic="0001" feature="F10" claim="B-019" />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var rules = entries.Read(Path.GetFullPath(settings.Root));
            if (settings.Json)
            {
                console.Profile.Out.Writer.WriteLine(JsonSerializer.Serialize(rules.Select(static rule => new
                {
                    id = rule.Id,
                    family = rule.Family,
                    defaultSeverity = rule.DefaultSeverity.ToString().ToLowerInvariant(),
                    effectiveLevel = rule.EffectiveLevel.ToString().ToLowerInvariant(),
                })));
                return Task.FromResult(ExitCodes.Success);
            }
            foreach (var rule in rules)
            {
                console.Profile.Out.Writer.WriteLine($"{rule.Id}\t{rule.Family}\t{rule.DefaultSeverity.ToString().ToLowerInvariant()}\t"
                    + rule.EffectiveLevel.ToString().ToLowerInvariant());
            }
            return Task.FromResult(ExitCodes.Success);
        }
        catch (SpechtRootNotFoundException)
        {
            return Fail(ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory.");
        }
        catch (SpechtManifestNotFoundException exception)
        {
            return Fail(ExitCodes.MissingInput, $"specht: {exception.Message}");
        }
        catch (Exception exception) when (exception is SpechtManifestUnreadableException or SpechtManifestException)
        {
            return Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }
    }

    /// <summary>The listing options.</summary>
    /// <implements epic="0001" feature="F10" claim="B-001" />
    public sealed class Settings : BranchSettings;

    /// <summary>The options shared by the rules branch and its default listing command.</summary>
    /// <implements epic="0001" feature="F10" claim="B-001" />
    public class BranchSettings : CommandSettings
    {
        /// <summary>Gets the root whose manifest supplies the pin and effective levels.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Gets whether the listing is written as one JSON document.</summary>
        [CommandOption("--json")]
        [Description("Print the listing as one JSON document.")]
        public bool Json { get; init; }
    }

    /// <implements epic="0001" feature="F10" claim="B-012" />
    /// <implements epic="0001" feature="F10" claim="B-013" />
    /// <implements epic="0001" feature="F10" claim="B-014" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
