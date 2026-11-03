using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Tree;
using Specht.Versioning;

namespace Specht.Tool.Features.Init;

/// <summary>
/// Writes the manifest and selected schemas and templates at the existing pin, or newest shipped version, under <c>&lt;root&gt;/.specht/</c>
/// (<c>0001-F4</c> B-029 to B-033), leaving every file that exists as it was (C-1).
/// </summary>
/// <param name="shippingCopy">
/// The embedded files by logical name: <c>manifest/v&lt;major.minor.patch&gt;/manifest.json</c>,
/// <c>schema/v&lt;major.minor.patch&gt;/frontmatter/&lt;file&gt;</c> or
/// <c>templates/v&lt;major.minor.patch&gt;/&lt;file&gt;</c>.
/// </param>
/// <param name="fileSystem">The file system the root is written through.</param>
/// <param name="manifestStore">The store for an existing manifest.</param>
/// <param name="upstreamSchemaFetcher">The fetcher needed when an upstream source selects schemas.</param>
/// <param name="manifestParser">The in-memory validator shared with manifest loading.</param>
/// <implements epic="0001" feature="F4" claim="B-019" />
public sealed partial class InitWriter(
    IReadOnlyDictionary<string, byte[]> shippingCopy,
    IFileSystem fileSystem,
    SpechtManifestParser manifestParser,
    SpechtManifestStore manifestStore,
    UpstreamSchemaFetcher? upstreamSchemaFetcher = null)
{
    /// <summary>Reads the shipping copy embedded in <paramref name="assembly"/>.</summary>
    /// <param name="assembly">The assembly carrying the <c>manifest/</c>, <c>schema/</c> and <c>templates/</c> resources.</param>
    /// <returns>Each shipped file's bytes by logical name.</returns>
    /// <implements epic="0001" feature="F4" claim="B-019" />
    public static IReadOnlyDictionary<string, byte[]> ShippingCopy(Assembly assembly) =>
        assembly.GetManifestResourceNames()
            .Where(static name => ShippedFiles.Pattern().IsMatch(name))
            .ToDictionary(static name => name, name => Read(assembly, name), StringComparer.Ordinal);

    /// <summary>Writes each absent selected file at the manifest pin, or newest shipped version for a new manifest.</summary>
    /// <param name="root">The repository root, which must be a directory.</param>
    /// <param name="schemas">Whether the frontmatter schemas are selected.</param>
    /// <param name="templates">Whether the templates are selected.</param>
    /// <param name="cancellationToken">Cancels the run between files.</param>
    /// <param name="upstream">The HTTPS base address to record in a new manifest.</param>
    /// <param name="upstreamVersion">The upstream version label.</param>
    /// <param name="upstreamHashes">Repeated kind=hash pairs for the declared schema kinds.</param>
    /// <param name="epics">Whether a new manifest keeps the epic grammar and the schemas include the epic schema.</param>
    /// <returns>Each file, relative to the root with <c>/</c> separators in ordinal order, and whether it was written or skipped.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <implements epic="0001" feature="F4" claim="B-019" />
    /// <implements epic="0001" feature="F4" claim="B-041" />
    /// <implements epic="0001" feature="F4" claim="B-018" />
    /// <implements epic="0001" feature="F4" claim="B-021" />
    /// <implements epic="0001" feature="F4" claim="B-023" />
    public async Task<IReadOnlyList<(string Path, bool Written)>> Write(
        string root,
        bool schemas,
        bool templates,
        CancellationToken cancellationToken,
        string? upstream = null,
        string? upstreamVersion = null,
        IReadOnlyList<string>? upstreamHashes = null,
        bool epics = false)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        var listing = new List<(string Path, bool Written)>();
        var manifestPresent = manifestStore.ManifestExists(root);
        var hasSourceOptions = upstream is not null || upstreamVersion is not null || upstreamHashes is { Count: > 0 };
        UpstreamSchemaSource? source = null;
        SemanticVersion? pinned = null;
        IReadOnlyList<string> consumerTemplates = [];
        var epicTier = epics;

        if (hasSourceOptions && manifestPresent)
        {
            throw new SpechtManifestException($"{SpechtManifestStore.RelativePath}: upstream options require a new manifest.");
        }

        if (manifestPresent)
        {
            try
            {
                var structure = manifestParser.Parse(manifestStore.ReadManifest(root));
                pinned = manifestParser.PinnedVersion(structure).Number;
                source = structure.SpechtSchema;
                consumerTemplates = structure.ConsumerTemplates;
                epicTier = structure.DeclaresEpicGrammar;
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
            {
                throw new SpechtManifestUnreadableException(exception);
            }
        }

        var selected = Selected(schemas, templates, epicTier, pinned).ToList();
        var manifestIndex = selected.FindIndex(static entry => entry.Relative == SpechtManifestStore.RelativePath);

        if (!manifestPresent && !epics && manifestIndex >= 0)
        {
            selected[manifestIndex] = (SpechtManifestStore.RelativePath, WithoutEpicGrammar(selected[manifestIndex].Bytes));
        }

        if (hasSourceOptions)
        {
            var hashes = new JsonObject();

            foreach (var pair in upstreamHashes ?? [])
            {
                var separator = pair.IndexOf('=');

                if (separator <= 0 || separator == pair.Length - 1)
                {
                    throw new SpechtManifestException("--upstream-hash must be written as kind=hash.");
                }

                var kind = pair[..separator];

                if (hashes.ContainsKey(kind))
                {
                    throw new SpechtManifestException("--upstream-hash repeats a kind.");
                }

                hashes[kind] = pair[(separator + 1)..];
            }

            var manifest = JsonNode.Parse(selected[manifestIndex].Bytes)!.AsObject();
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = upstream,
                ["version"] = upstreamVersion,
                ["hashes"] = hashes,
            };
            var text = manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            source = manifestParser.Parse(text).SpechtSchema;
            selected[manifestIndex] = (SpechtManifestStore.RelativePath, System.Text.Encoding.UTF8.GetBytes(text + "\n"));
        }

        var preserved = new HashSet<string>(StringComparer.Ordinal);

        if (schemas && source is not null)
        {
            selected.RemoveAll(static entry => entry.Relative.StartsWith(SpechtManifestStore.FrontmatterSchemaFolder + "/", StringComparison.Ordinal));

            foreach (var file in source.Files)
            {
                var relative = $"{SpechtManifestStore.FrontmatterSchemaFolder}/{file.FileName}";
                var path = fileSystem.Path.Combine([root, .. relative.Split('/')]);
                var withinSchemas = fileSystem.Path.GetRelativePath(
                    fileSystem.Path.Combine([root, .. SpechtManifestStore.FrontmatterSchemaFolder.Split('/')]), path);

                if (fileSystem.Path.IsPathRooted(file.FileName)
                    || fileSystem.Path.IsPathRooted(withinSchemas)
                    || withinSchemas == ".."
                    || withinSchemas.StartsWith(".." + fileSystem.Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    || withinSchemas.StartsWith(".." + fileSystem.Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
                {
                    throw new SpechtManifestException(
                        $"{SpechtManifestStore.RelativePath}: frontmatterSchemas must name files within the frontmatter schema folder.");
                }

                var exists = fileSystem.File.Exists(path);

                if (exists)
                {
                    preserved.Add(relative);
                }

                var bytes = exists
                    ? []
                    : await (upstreamSchemaFetcher ?? throw new InvalidOperationException("An upstream schema fetcher is required."))
                        .Fetch(file, cancellationToken);
                selected.Add((relative, bytes));
            }
        }

        if (templates && source is not null)
        {
            foreach (var file in source.Templates.Where(file => !consumerTemplates.Contains(file.FileName, StringComparer.Ordinal)))
            {
                var relative = $"{SpechtManifestStore.TemplateFolder}/{file.FileName}";

                if (fileSystem.File.Exists(fileSystem.Path.Combine([root, .. relative.Split('/')])))
                {
                    continue;
                }

                var bytes = await (upstreamSchemaFetcher ?? throw new InvalidOperationException("An upstream schema fetcher is required."))
                    .Fetch(file, cancellationToken);
                selected.RemoveAll(entry => entry.Relative == relative);
                selected.Add((relative, bytes));
            }
        }

        foreach (var (relative, bytes) in selected.OrderBy(static entry => entry.Relative, StringComparer.Ordinal))
        {
            var path = fileSystem.Path.Combine([root, .. relative.Split('/')]);

            if (preserved.Contains(relative) || fileSystem.File.Exists(path))
            {
                listing.Add((relative, false));
                continue;
            }

            fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);
            await using (var stream = fileSystem.FileStream.New(path, FileMode.CreateNew, FileAccess.Write))
            {
                await stream.WriteAsync(bytes, cancellationToken);
            }

            listing.Add((relative, true));
        }

        return listing;
    }

    /// <implements epic="0001" feature="F4" claim="B-019" />
    private static byte[] Read(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);

        return buffer.ToArray();
    }

    /// <summary>
    /// Removes the epic grammar from <paramref name="manifest"/>'s bytes and, where its task grammar names an
    /// <c>epic</c> group, writes <see cref="EpiclessTaskGrammar"/> of it in its place; every other byte is kept.
    /// </summary>
    /// <param name="manifest">The shipped manifest's bytes.</param>
    /// <returns>The manifest without the epic grammar.</returns>
    /// <implements epic="0001" feature="F4" claim="B-003" />
    /// <implements epic="0001" feature="F4" claim="B-021" />
    private static byte[] WithoutEpicGrammar(byte[] manifest)
    {
        var reader = new Utf8JsonReader(manifest);
        var members = new List<(string Name, int Start, int ValueStart, int End, string? Value)>();

        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            return manifest;
        }

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var identifiers = reader.ValueTextEquals("identifiers"u8);
            reader.Read();

            if (identifiers && reader.TokenType == JsonTokenType.StartObject)
            {
                while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
                {
                    var name = reader.GetString()!;
                    var start = (int)reader.TokenStartIndex;
                    reader.Read();
                    var valueStart = (int)reader.TokenStartIndex;
                    var value = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    reader.Skip();
                    members.Add((name, start, valueStart, (int)reader.BytesConsumed, value));
                }

                break;
            }

            reader.Skip();
        }

        var epic = members.FindIndex(static member => member.Name == "epic");

        if (epic < 0)
        {
            return manifest;
        }

        var (from, to) = epic > 0
            ? (members[epic - 1].End, members[epic].End)
            : members.Count > 1 ? (members[0].Start, members[1].Start) : (members[0].Start, members[0].End);
        var task = members.FindIndex(static member => member.Name == "task"
            && member.Value is { } grammar && EpicGroup().IsMatch(grammar));
        var splices = new List<(int From, int To, byte[] Bytes)> { (from, to, []) };
        if (task >= 0)
        {
            var (valueStart, end) = (members[task].ValueStart, members[task].End);
            splices.Add((valueStart, end, JsonSerializer.SerializeToUtf8Bytes(EpiclessTaskGrammar(members[task].Value!), GrammarJson)));
        }

        var written = manifest;
        foreach (var (start, end, bytes) in splices.OrderByDescending(static splice => splice.From))
        {
            written = [.. written.AsSpan(0, start), .. bytes, .. written.AsSpan(end)];
        }

        return written;
    }

    /// <summary>
    /// The task grammar a manifest written without the epic grammar declares in place of one holding an <see cref="EpicGroup"/>:
    /// the shipped grammar with that group's parentheses dropped and its pattern kept (<c>0001-F5</c> OQ-5), so
    /// <c>^(?&lt;epic&gt;[0-9]{4})-(?&lt;sequence&gt;[0-9]{2})$</c> becomes <c>^[0-9]{4}-(?&lt;sequence&gt;[0-9]{2})$</c>.
    /// </summary>
    /// <param name="shipped">The shipped task grammar, holding an <see cref="EpicGroup"/>.</param>
    /// <returns>The grammar with each <see cref="EpicGroup"/> match replaced by its <c>pattern</c>.</returns>
    /// <implements epic="0001" feature="F4" claim="B-022" />
    private static string EpiclessTaskGrammar(string shipped) => EpicGroup().Replace(shipped, "${pattern}");

    /// <summary>
    /// Matches the one shape of <c>epic</c> group a shipped task grammar has: <c>(?&lt;epic&gt;</c>, a <c>pattern</c> holding
    /// no parenthesis, and <c>)</c>.
    /// </summary>
    /// <returns>The shared generated expression.</returns>
    /// <implements epic="0001" feature="F4" claim="B-022" />
    [GeneratedRegex(@"\(\?<epic>(?<pattern>[^()]*)\)")]
    private static partial Regex EpicGroup();

    /// <implements epic="0001" feature="F4" claim="B-019" />
    /// <implements epic="0001" feature="F4" claim="B-023" />
    private IEnumerable<(string Relative, byte[] Bytes)> Selected(bool schemas, bool templates, bool epicTier, SemanticVersion? pinned)
    {
        var shipped = new ShippedFiles(shippingCopy);

        return shipped.At(pinned ?? shipped.Newest())
            .Where(file => file.Relative switch
            {
                SpechtManifestStore.RelativePath => true,
                var relative when relative.StartsWith(SpechtManifestStore.ConfigurationFolder + "/schema/", StringComparison.Ordinal) =>
                    schemas && (epicTier || relative != EpicSchemaRelative),
                var relative when relative.StartsWith(SpechtManifestStore.TemplateFolder + "/", StringComparison.Ordinal) => templates,
                _ => false,
            })
            .Select(static file => (file.Relative, file.Bytes));
    }

    private const string EpicSchemaRelative = SpechtManifestStore.FrontmatterSchemaFolder + "/epic.schema.json";

    private static readonly JsonSerializerOptions GrammarJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
}
