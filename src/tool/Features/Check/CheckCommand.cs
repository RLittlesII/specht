using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Specht.Evaluation;
using Specht.Manifest;
using Specht.Report;
using Specht.Tree;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Check;

/// <summary>
/// <c>specht</c>, the default command (<c>0001-F2</c>): checks the tree under a root, prints one MSBuild-shaped line per
/// violation on stdout and then the run summary - or, under <c>--json</c>, the report document in their place (<c>0001-F3</c>
/// B-001) - and, given <c>--report</c>, writes the same document to that path (<c>0001-F3</c> B-003, B-022), and folds the
/// report into an exit code. It parses, calls the engine's check and folds - nothing else (C-4).
/// </summary>
/// <param name="console">Where the product goes; injected so the command tester captures it.</param>
/// <param name="explanations">The engine's pinned rule page reader.</param>
/// <param name="run">The engine's check, <see cref="Specht.Check.SpechtCheck.Run(string, SchemaSourceSelection?)"/> outside a test.</param>
/// <param name="mapper">Makes the report document from the report.</param>
/// <param name="serializer">Serializes the report document.</param>
/// <param name="store">Writes the report document to the <c>--report</c> path, and removes it from a run that checks nothing.</param>
/// <implements epic="0001" feature="F2" claim="B-001" />
/// <implements epic="0001" feature="F3" claim="B-003" />
public sealed class CheckCommand(IAnsiConsole console, Func<string, SchemaSourceSelection?, SpechtReport> run,
    RuleExplanationReader explanations, SpechtReportDocumentMapper mapper, SpechtReportSerializer serializer, SpechtReportStore store)
    : AsyncCommand<CheckCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0001" feature="F2" claim="B-001" />
    /// <implements epic="0001" feature="F3" claim="B-003" />
    /// <implements epic="0001" feature="F3" claim="B-004" />
    /// <implements epic="0001" feature="F3" claim="B-025" />
    /// <implements epic="0001" feature="F3" claim="B-044" />
    /// <implements epic="0001" feature="F3" claim="B-045" />
    /// <implements epic="0001" feature="F3" claim="B-046" />
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
            return Unchecked(settings, ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory.");
        }
        catch (SpechtManifestNotFoundException exception)
        {
            return Unchecked(settings, ExitCodes.MissingInput, $"specht: {exception.Message}");
        }
        catch (SpechtManifestUnreadableException exception)
        {
            return Unchecked(settings, ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }
        catch (SpechtManifestException exception)
        {
            return Unchecked(settings, ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }

        var document = mapper.Map(report);

        if (settings.Report is { } path)
        {
            store.Save(serializer.Serialize(document), path);
        }

        if (settings.Json)
        {
            console.Profile.Out.Writer.WriteLine(serializer.Serialize(document));
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

        /// <summary>Gets the path the report document is written to, relative to the working directory; none when omitted.</summary>
        [CommandOption("--report <PATH>")]
        [Description("Also write the report document to this file, relative to the working directory.")]
        public string? Report { get; init; }

        /// <summary>Gets the explicit frontmatter source for this invocation, overriding a valid manifest selection.</summary>
        [CommandOption("--schema-source <embedded|disk>")]
        [Description("Select embedded or local disk frontmatter schemas for this check.")]
        public string? SchemaSource { get; init; }

        /// <summary>Gets the rule id whose entire page is printed without running a check.</summary>
        [CommandOption("--explain <rule id>")]
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

    /// <implements epic="0001" feature="F3" claim="B-044" />
    /// <implements epic="0001" feature="F3" claim="B-045" />
    private Task<int> Unchecked(Settings settings, int code, string message)
    {
        if (settings.Report is { } path)
        {
            store.Delete(path);
        }

        return Fail(code, message);
    }
}
