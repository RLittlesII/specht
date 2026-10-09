using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace specht.tool.Features.Check;

/// <summary>
/// <c>specht</c>, the default command (<c>0001-F2</c>): checks the tree under a root, prints one MSBuild-shaped line per
/// violation on stdout, and folds the report into an exit code. It parses, calls the runner and folds - nothing else (C-4).
/// </summary>
/// <param name="console">Where the product goes; injected so the command tester captures it.</param>
/// <param name="run">The engine's runner, <see cref="SpecCheckRunner.Run"/> outside a test.</param>
public sealed class CheckCommand(IAnsiConsole console, Func<string, SpecCheckReport> run) : AsyncCommand<CheckCommand.Settings>
{
    /// <inheritdoc />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        SpecCheckReport report;

        try
        {
            report = run(Path.GetFullPath(settings.Root));
        }
        catch (SpechtRootNotFoundException)
        {
            return Fail(ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory.");
        }
        catch (SpechtManifestNotFoundException exception)
        {
            return Fail(ExitCodes.MissingInput, $"specht: {exception.Message}");
        }
        catch (SpechtManifestUnreadableException exception)
        {
            return Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }

        foreach (var violation in report.Violations)
        {
            console.Profile.Out.Writer.WriteLine(violation);
        }

        var failed = settings.Strict ? report.Violations.Count > 0 : report.ErrorCount > 0;

        return Task.FromResult(failed ? ExitCodes.Violations : ExitCodes.Success);
    }

    /// <summary>The check's options.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root to check, as typed; the working directory when omitted.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root to check. Defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Gets a value indicating whether any violation fails the run, not only an error.</summary>
        [CommandOption("--strict")]
        [Description("Fail on a violation of any severity, not only an error.")]
        public bool Strict { get; init; }
    }

    private static Task<int> Fail(int code, string message)
    {
        Console.Error.WriteLine(message);

        return Task.FromResult(code);
    }
}
