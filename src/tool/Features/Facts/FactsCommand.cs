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

/// <summary>
/// Gives the specification tree's facts, read as the check reads the tree: one line per fact by default, or one JSON
/// document with <c>--json</c>.
/// </summary>
/// <param name="console">The standard output.</param>
/// <param name="reader">The check's own read of the tree.</param>
/// <param name="errors">Standard error, or the injected writer.</param>
/// <implements epic="0132" feature="F2" claim="B-001" />
/// <implements epic="0132" feature="F2" claim="B-024" />
public sealed class FactsCommand(IAnsiConsole console, SpechtModelReader reader, TextWriter? errors = null)
    : AsyncCommand<FactsCommand.Settings>
{
    /// <inheritdoc />
    /// <implements epic="0132" feature="F2" claim="B-001" />
    /// <implements epic="0132" feature="F2" claim="B-002" />
    /// <implements epic="0132" feature="F2" claim="B-011" />
    /// <implements epic="0132" feature="F2" claim="B-018" />
    /// <implements epic="0132" feature="F2" claim="B-019" />
    /// <implements epic="0132" feature="F2" claim="B-020" />
    /// <implements epic="0132" feature="F2" claim="B-021" />
    /// <implements epic="0132" feature="F2" claim="B-023" />
    /// <implements epic="0132" feature="F2" claim="B-024" />
    public override Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var root = Path.GetFullPath(settings.Root);
            var model = reader.Read(root, null);
            var document = FactsDocument.From(model, reader.ReadRecords(root, model.Features));
            if (settings.Path is { } path)
            {
                document = document.Select(path);
                if (document.Specifications.Count == 0)
                {
                    return Fail(ExitCodes.NotFound, $"specht: '{path}' selects no specification.");
                }
            }

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

    /// <summary>The facts options.</summary>
    /// <implements epic="0132" feature="F2" claim="B-001" />
    public sealed class Settings : CommandSettings
    {
        /// <summary>Gets the root-relative path whose specifications are given, as typed, or <see langword="null"/> for every one.</summary>
        /// <implements epic="0132" feature="F2" claim="B-018" />
        /// <implements epic="0132" feature="F2" claim="B-019" />
        /// <implements epic="0132" feature="F2" claim="B-020" />
        [CommandArgument(0, "[path]")]
        [Description("A specification's path, or a directory, relative to the root. Defaults to every specification.")]
        public string? Path { get; init; }

        /// <summary>Gets the root whose specification tree is read.</summary>
        [CommandOption("--root <DIR>")]
        [Description("The repository root. Defaults to the working directory.")]
        public string Root { get; init; } = ".";

        /// <summary>Gets whether the facts are written as one JSON document rather than the default, one line per fact.</summary>
        /// <implements epic="0132" feature="F2" claim="B-024" />
        [CommandOption("--json")]
        [Description("Print the facts as one JSON document.")]
        public bool Json { get; init; }
    }

    /// <implements epic="0132" feature="F2" claim="B-021" />
    private async Task<int> Fail(int code, string message)
    {
        await (errors ?? Console.Error).WriteLineAsync(message);
        return code;
    }
}
