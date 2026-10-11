using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using Specht.Report;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Check;

/// <summary>
/// <c>specht</c>, the default command (<c>0001-F2</c>): checks the tree under a root, prints one MSBuild-shaped line per
/// violation on stdout and then the run summary - or, under <c>--json</c>, the report document in their place (<c>0001-F3</c>
/// B-001) - and folds the report into an exit code. It parses, calls the runner and folds - nothing else (C-4).
/// </summary>
/// <param name="console">Where the product goes; injected so the command tester captures it.</param>
/// <param name="run">The engine's runner, <see cref="SpechtRunner.Run(string)"/> outside a test.</param>
public sealed class CheckCommand(IAnsiConsole console, Func<string, Either<SpechtFailure, SpechtReport>> run)
    : AsyncCommand<CheckCommand.Settings>
{
    /// <inheritdoc />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken) =>
        Task.FromResult(
            run(Path.GetFullPath(settings.Root)).Match(
                Left: failure => Fail(failure, settings.Root),
                Right: report => Print(report, settings)));

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

        /// <summary>Gets a value indicating whether stdout carries the report document instead of the lines and the summary.</summary>
        [CommandOption("--json")]
        [Description("Write the report document to stdout in place of the violation lines and the summary.")]
        public bool Json { get; init; }
    }

    private static int Fail(SpechtFailure failure, string root)
    {
        Console.Error.WriteLine(
            failure.Kind is SpechtFailureKind.RootNotFound ? $"specht: '{root}' is not a directory." : $"specht: {failure.Message}");

#pragma warning disable CS8524 // An unnamed kind is a defect; a named kind left out still fails the build as CS8509.
        return failure.Kind switch
        {
            SpechtFailureKind.RootNotFound => ExitCodes.MissingInput,
            SpechtFailureKind.ManifestNotFound => ExitCodes.MissingInput,
            SpechtFailureKind.ManifestUnreadable => ExitCodes.InvalidManifest,
            SpechtFailureKind.ManifestRejected => ExitCodes.InvalidManifest,
        };
#pragma warning restore CS8524
    }

    private int Print(SpechtReport report, Settings settings)
    {
        var document = SpecReportDocument.From(report);

        if (settings.Json)
        {
            console.Profile.Out.Writer.WriteLine(document.ToJson());
        }
        else
        {
            foreach (var violation in report.Violations)
            {
                console.Profile.Out.Writer.WriteLine(violation);
            }

            foreach (var line in document.SummaryLines())
            {
                console.Profile.Out.Writer.WriteLine(line);
            }
        }

        var failed = settings.Strict ? report.Violations.Count > 0 : report.ErrorCount > 0;

        return failed ? ExitCodes.Violations : ExitCodes.Success;
    }
}
