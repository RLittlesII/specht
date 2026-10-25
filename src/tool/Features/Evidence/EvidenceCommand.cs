using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Evidence;

/// <summary>The offline gate over explicitly supplied trace evidence (0201-F5).</summary>
/// <implements epic="0201" feature="F5" claim="B-014" />
public sealed class EvidenceCommand(IAnsiConsole console, EvidenceReconciler reconciler) : Command<EvidenceCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0201" feature="F5" claim="B-014" />
    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var root = Path.GetFullPath(settings.Root);
        var result = reconciler.Reconcile(root, settings.Paths, settings.Run);
        var json = result.Report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;
        if (settings.Report is not null)
        {
            try
            {
                File.WriteAllText(settings.Report, json);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                Console.Error.WriteLine("specht: the requested evidence report could not be written.");
                return 4;
            }
        }

        if (settings.Json)
        {
            console.Profile.Out.Writer.Write(json);
        }
        else
        {
            var summary = result.Report["summary"]!;
            console.Profile.Out.Writer.WriteLine(
                $"claims: {summary["selectedClaims"]}, covered: {summary["coveredClaims"]}, exempt: {summary["exemptClaims"]}");
            var inputs = summary["inputs"]!;
            var bindings = summary["bindings"]!;
            var links = summary["links"]!;
            var execution = summary["execution"]!;
            console.Profile.Out.Writer.WriteLine(
                $"inputs: current {inputs["current"]}, stale {inputs["stale"]}, unavailable {inputs["unavailable"]}");
            console.Profile.Out.Writer.WriteLine(
                $"bindings: resolved {bindings["resolved"]}, unresolved {bindings["unresolved"]}, invalid {bindings["invalid"]}, "
                + $"ambiguous {bindings["ambiguous"]}, ineligible {bindings["ineligible"]}");
            console.Profile.Out.Writer.WriteLine(
                $"links: satisfied {links["satisfied"]}, missing {links["missing"]}, exempt {links["exempt"]}, "
                + $"unavailable {links["unavailable"]}");
            console.Profile.Out.Writer.WriteLine(
                $"passed: {execution["passed"]}, failed: {execution["failed"]}, skipped: {execution["skipped"]}, "
                + $"notRun: {execution["notRun"]}, aborted: {execution["aborted"]}, absent: {execution["absent"]}");
            console.Profile.Out.Writer.WriteLine($"findings: {summary["findings"]}");
            foreach (var finding in result.Report["findings"]!.AsArray())
            {
                var path = finding!["path"]!.GetValue<string>();
                var kind = finding["kind"]!.GetValue<string>();
                var message = finding["message"]!.GetValue<string>();
                console.Profile.Out.Writer.WriteLine($"{path}: {kind}: {message}");
            }
        }

        return result.ExitCode;
    }

    /// <summary>Explicit scope and output selection.</summary>
    /// <implements epic="0201" feature="F5" claim="B-014" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>The repository root.</summary>
        [CommandOption("--root <DIR>")]
        [Description("Repository root; defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Evidence files or directories named positionally.</summary>
        [CommandArgument(0, "[PATH]")]
        [Description("Evidence file or directory to reconcile; may be repeated.")]
        public string[] Paths { get; init; } = [];

        /// <summary>The run identity when several are supplied.</summary>
        [CommandOption("--run <ID>")]
        [Description("Select a runner invocation by its exact run ID.")]
        public string? Run { get; init; }

        /// <summary>Write JSON on stdout.</summary>
        [CommandOption("--json")]
        [Description("Write the versioned evidence report as JSON.")]
        public bool Json { get; init; }

        /// <summary>Write an identical report to a caller-selected file.</summary>
        [CommandOption("--report <PATH>")]
        [Description("Write the JSON report to this path.")]
        public string? Report { get; init; }
    }
}
