using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Facts;
using Specht.Manifest;
using Specht.Model;
using Specht.Tree;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Facts;

/// <summary>Gives the specification tree's facts as one JSON document, read as the check reads the tree.</summary>
/// <param name="console">The standard output.</param>
/// <param name="reader">The check's own read of the tree.</param>
/// <param name="errors">Standard error, or the injected writer.</param>
/// <implements epic="0132" feature="F2" claim="B-001" />
public sealed class FactsCommand(IAnsiConsole console, SpechtModelReader reader, TextWriter? errors = null)
    : AsyncCommand<FactsCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0132" feature="F2" claim="B-001" />
    /// <implements epic="0132" feature="F2" claim="B-002" />
    /// <implements epic="0132" feature="F2" claim="B-021" />
    /// <implements epic="0132" feature="F2" claim="B-023" />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!settings.Json)
        {
            return Fail(ExitCodes.MissingInput, "specht facts [--root <dir>] --json");
        }

        try
        {
            console.Profile.Out.Writer.WriteLine(FactsDocument.From(reader.Read(Path.GetFullPath(settings.Root), null)).ToJson());
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

    /// <summary>The facts options.</summary>
    /// <implements epic="0132" feature="F2" claim="B-001" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root whose specification tree is read.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Gets whether the facts are written as one JSON document, the only form until the text form lands.</summary>
        [CommandOption("--json")]
        [Description("Print the facts as one JSON document. Required.")]
        public bool Json { get; init; }
    }

    /// <implements epic="0132" feature="F2" claim="B-021" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
