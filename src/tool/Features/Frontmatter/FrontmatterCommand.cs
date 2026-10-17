using System;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Specht.Tool.Features.Frontmatter;

/// <summary>Checks one document through the engine and prints its frontmatter verdict.</summary>
/// <param name="console">The diagnostic output.</param>
/// <param name="runner">The existing check engine.</param>
/// <param name="files">The document's file system.</param>
/// <param name="input">Standard input, or the injected reader.</param>
/// <param name="errors">Standard error, or the injected writer.</param>
public sealed class FrontmatterCommand(IAnsiConsole console, SpechtRunner runner, IFileSystem files, TextReader? input = null, TextWriter? errors = null)
    : AsyncCommand<FrontmatterCommand.Settings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Kind is null || (settings.DocumentPath is null) == !settings.Stdin)
        {
            return await Fail(ExitCodes.MissingInput, "specht frontmatter --kind <kind> (<path> | --stdin) [--root <dir>]");
        }

        FrontmatterKind kind;
        try
        {
            kind = runner.ParseFrontmatterKind(settings.Kind);
        }
        catch (ArgumentException exception)
        {
            return await Fail(ExitCodes.NotFound, $"specht: {exception.Message}");
        }

        if (!settings.Stdin && !files.File.Exists(settings.DocumentPath))
        {
            return await Fail(ExitCodes.NotFound, $"specht: '{settings.DocumentPath}' is not a file.");
        }

        var text = settings.Stdin
            ? await (input ?? Console.In).ReadToEndAsync(cancellationToken)
            : await files.File.ReadAllTextAsync(settings.DocumentPath!, cancellationToken);
        SpechtReport report;
        try
        {
            report = runner.CheckFrontmatter(files.Path.GetFullPath(settings.Root), kind, settings.DocumentPath ?? "<stdin>", text);
        }
        catch (SpechtRootNotFoundException)
        {
            return await Fail(ExitCodes.MissingInput, $"specht: '{settings.Root}' is not a directory.");
        }
        catch (SpechtManifestNotFoundException exception)
        {
            return await Fail(ExitCodes.MissingInput, $"specht: {exception.Message}");
        }
        catch (Exception exception) when (exception is SpechtManifestUnreadableException or SpechtManifestException)
        {
            return await Fail(ExitCodes.InvalidManifest, $"specht: {exception.Message}");
        }

        foreach (var violation in report.Violations)
        {
            console.Profile.Out.Writer.WriteLine(violation);
        }
        console.Profile.Out.Writer.WriteLine(SpecReportDocument.From(report).SummaryLines()[^1]);
        return report.ErrorCount > 0 ? ExitCodes.Violations : ExitCodes.Success;
    }

    /// <summary>The frontmatter command's options.</summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the document path exactly as typed.</summary>
        [CommandArgument(0, "[path]")]
        [Description("The document to check; use a path or --stdin.")]
        public string? DocumentPath { get; init; }

        /// <summary>Gets the frontmatter kind.</summary>
        [CommandOption("--kind <KIND>")]
        [Description("The frontmatter schema kind: feature, task or epic.")]
        public string? Kind { get; init; }

        /// <summary>Gets whether to read the document from standard input.</summary>
        [CommandOption("--stdin")]
        [Description("Read the document from standard input instead of a path.")]
        public bool Stdin { get; init; }

        /// <summary>Gets the root whose manifest selects the schema.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";
    }

    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
