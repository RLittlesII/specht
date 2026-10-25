using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
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
public sealed class InitWriter(
    IReadOnlyDictionary<string, byte[]> shippingCopy,
    IFileSystem fileSystem,
    SpecManifestParser manifestParser,
    SpecManifestStore manifestStore,
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
    /// <returns>Each file, relative to the root with <c>/</c> separators in ordinal order, and whether it was written or skipped.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <implements epic="0001" feature="F4" claim="B-019" />
    public async Task<IReadOnlyList<(string Path, bool Written)>> Write(
        string root,
        bool schemas,
        bool templates,
        CancellationToken cancellationToken,
        string? upstream = null,
        string? upstreamVersion = null,
        IReadOnlyList<string>? upstreamHashes = null)
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

        if (hasSourceOptions && manifestPresent)
        {
            throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: upstream options require a new manifest.");
        }

        if (manifestPresent)
        {
            try
            {
                var structure = manifestParser.Parse(manifestStore.ReadManifest(root));
                pinned = manifestParser.PinnedVersion(structure).Number;
                source = structure.SpechtSchema;
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException)
            {
                throw new SpechtManifestUnreadableException(exception);
            }
        }

        var selected = Selected(schemas, templates, manifestPresent, pinned).ToList();

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

            var manifestIndex = selected.FindIndex(static entry => entry.Relative == SpecManifestStore.RelativePath);
            var manifest = JsonNode.Parse(selected[manifestIndex].Bytes)!.AsObject();
            manifest["identifiers"]?.AsObject().Remove("epic");
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = upstream,
                ["version"] = upstreamVersion,
                ["hashes"] = hashes,
            };
            var text = manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            source = manifestParser.Parse(text).SpechtSchema;
            selected[manifestIndex] = (SpecManifestStore.RelativePath, System.Text.Encoding.UTF8.GetBytes(text + "\n"));
        }

        var preserved = new HashSet<string>(StringComparer.Ordinal);

        if (schemas && source is not null)
        {
            selected.RemoveAll(static entry => entry.Relative.StartsWith(SpecManifestStore.FrontmatterSchemaFolder + "/", StringComparison.Ordinal));

            foreach (var file in source.Files)
            {
                var relative = $"{SpecManifestStore.FrontmatterSchemaFolder}/{file.FileName}";
                var path = fileSystem.Path.Combine([root, .. relative.Split('/')]);
                var withinSchemas = fileSystem.Path.GetRelativePath(
                    fileSystem.Path.Combine([root, .. SpecManifestStore.FrontmatterSchemaFolder.Split('/')]), path);

                if (fileSystem.Path.IsPathRooted(file.FileName)
                    || fileSystem.Path.IsPathRooted(withinSchemas)
                    || withinSchemas == ".."
                    || withinSchemas.StartsWith(".." + fileSystem.Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    || withinSchemas.StartsWith(".." + fileSystem.Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
                {
                    throw new SpechtManifestException(
                        $"{SpecManifestStore.RelativePath}: frontmatterSchemas must name files within the frontmatter schema folder.");
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

    /// <implements epic="0001" feature="F4" claim="B-019" />
    private IEnumerable<(string Relative, byte[] Bytes)> Selected(bool schemas, bool templates, bool manifestPresent, SemanticVersion? pinned)
    {
        var shipped = new ShippedFiles(shippingCopy);

        return shipped.At(pinned ?? shipped.Newest())
            .Where(file => file.Relative switch
            {
                SpecManifestStore.RelativePath => true,
                var relative when relative.StartsWith(SpecManifestStore.ConfigurationFolder + "/schema/", StringComparison.Ordinal) =>
                    schemas && (manifestPresent || relative != EpicSchemaRelative),
                var relative when relative.StartsWith(SpecManifestStore.TemplateFolder + "/", StringComparison.Ordinal) => templates,
                _ => false,
            })
            .Select(static file => (file.Relative, file.Bytes));
    }

    private const string EpicSchemaRelative = SpecManifestStore.FrontmatterSchemaFolder + "/epic.schema.json";
}
