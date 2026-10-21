using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
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
/// <param name="explanations">The engine's pinned rule page reader.</param>
/// <param name="run">The engine's runner, <see cref="SpechtRunner.Run(string, SchemaSourceSelection?)"/> outside a test.</param>
/// <implements epic="0001" feature="F2" claim="B-001" />
public sealed class CheckCommand(IAnsiConsole console, Func<string, SchemaSourceSelection?, SpechtReport> run,
    RuleExplanationReader explanations) : AsyncCommand<CheckCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F2" claim="B-001" />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        SpechtReport report;

        try
        {
            var root = Path.GetFullPath(settings.Root);
            if (settings.Explain is { } id)
            {
                var page = explanations.Read(root, id);
                if (page is null)
                {
                    return Fail(ExitCodes.NotFound, $"specht: rule '{id}' was not found in the pinned schema version.");
                }

                console.Profile.Out.Writer.Write(page);
                return Task.FromResult(ExitCodes.Success);
            }

            report = run(root, settings.SchemaSource switch
            {
                "embedded" => SchemaSourceSelection.Embedded,
                "disk" => SchemaSourceSelection.Disk,
                _ => null,
            });
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
        catch (SpechtManifestException exception)
        {
            return Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }

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

        return Task.FromResult(failed ? ExitCodes.Violations : ExitCodes.Success);
    }

    /// <summary>The check's options.</summary>
    /// <implements epic="0001" feature="F2" claim="B-001" />
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

        /// <summary>Gets the explicit frontmatter source for this invocation, overriding a valid manifest selection.</summary>
        [CommandOption("--schema-source <embedded|disk>")]
        [Description("Select embedded or local disk frontmatter schemas for this check.")]
        public string? SchemaSource { get; init; }

        /// <summary>Gets the rule id whose entire page is printed without running a check.</summary>
        [CommandOption("--explain <SPEC###>")]
        [Description("Print the entire rule page for the pinned schema version without running a check.")]
        public string? Explain { get; init; }

        /// <inheritdoc />
        /// <implements epic="0001" feature="F2" claim="B-001" />
        public override ValidationResult Validate() => SchemaSource is null or "embedded" or "disk"
            ? ValidationResult.Success()
            : ValidationResult.Error("The schema source must be 'embedded' or 'disk'.");
    }

    /// <implements epic="0001" feature="F2" claim="B-001" />
    private static Task<int> Fail(int code, string message)
    {
        Console.Error.WriteLine(message);

        return Task.FromResult(code);
    }
}
