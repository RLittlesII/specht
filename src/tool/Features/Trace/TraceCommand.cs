using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Model;
using Specht.Rules.Matrix;
using Specht.Trace;
using Specht.Tree;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Trace;

/// <summary>
/// Prints where every claim stands, read as the check reads the tree and resolved through the check's matrix resolver: one
/// line per claim by default, or one JSON document with <c>--json</c>. No rule runs, so it exits <c>0</c> whatever the
/// trace shows.
/// </summary>
/// <param name="console">The standard output.</param>
/// <param name="reader">The check's own read of the tree.</param>
/// <param name="resolver">The check's matrix resolver.</param>
/// <param name="errors">Standard error, or the injected writer.</param>
/// <implements epic="0001" feature="F13" claim="B-001" />
/// <implements epic="0001" feature="F13" claim="B-002" />
/// <implements epic="0001" feature="F13" claim="B-006" />
public sealed class TraceCommand(IAnsiConsole console, SpechtModelReader reader, MatrixResolver resolver, TextWriter? errors = null)
    : AsyncCommand<TraceCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F13" claim="B-001" />
    /// <implements epic="0001" feature="F13" claim="B-002" />
    /// <implements epic="0001" feature="F13" claim="B-006" />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var document = TraceDocument.From(reader.Read(Path.GetFullPath(settings.Root), null), resolver);
            if (settings.Json)
            {
                console.Profile.Out.Writer.WriteLine(document.ToJson());
            }
            else
            {
                foreach (var line in document.Lines())
                {
                    console.Profile.Out.Writer.WriteLine(line);
                }
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

    /// <summary>The trace options.</summary>
    /// <implements epic="0001" feature="F13" claim="B-001" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root whose specification tree is traced.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Gets whether the trace is written as one JSON document rather than the default, one line per claim.</summary>
        /// <implements epic="0001" feature="F13" claim="B-002" />
        [CommandOption("--json")]
        [Description("Print the trace as one JSON document.")]
        public bool Json { get; init; }
    }

    /// <implements epic="0001" feature="F13" claim="B-006" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
