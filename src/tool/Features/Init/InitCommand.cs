using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Init;

/// <summary>
/// <c>specht init</c> (<c>0001-F4</c>): writes the newest shipped schema set and templates under a root, lists each of the
/// eight files relative to the root as written or skipped (B-006), and folds the run into an exit code. It parses, calls
/// the writer, and folds - nothing else.
/// </summary>
/// <param name="console">Where the listing goes; injected so the command tester captures it.</param>
/// <param name="writer">The writer over the embedded shipping copy and the real file system outside a test.</param>
public sealed class InitCommand(IAnsiConsole console, InitWriter writer) : AsyncCommand<InitCommand.Settings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var (path, written) in await writer.Write(Path.GetFullPath(settings.Root), cancellationToken))
            {
                console.Profile.Out.Writer.WriteLine($"{(written ? "written" : "skipped")} {path}");
            }
        }
        catch (SpechtRootNotFoundException)
        {
            await Console.Error.WriteLineAsync($"specht: '{settings.Root}' is not a directory.");

            return ExitCodes.MissingInput;
        }

        return ExitCodes.Success;
    }

    /// <summary>The init command's options.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root to write under, as typed; the working directory when omitted.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root to write the schema set and templates under. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }
}
