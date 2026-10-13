using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Resources;
using Specht.Versioning;

namespace Specht.Tool.Features.Init;

/// <summary>
/// Writes the newest shipped manifest, and the selected schemas and templates, under <c>&lt;root&gt;/.specht/</c>
/// (<c>0001-F4</c> B-029 to B-033), leaving every file that exists as it was (C-1).
/// </summary>
/// <param name="shippingCopy">
/// The embedded files by logical name: <c>manifest/v&lt;major.minor.patch&gt;/manifest.json</c>,
/// <c>schema/v&lt;major.minor.patch&gt;/frontmatter/&lt;file&gt;</c> or
/// <c>templates/v&lt;major.minor.patch&gt;/&lt;file&gt;</c>.
/// </param>
/// <param name="fileSystem">The file system the root is written through.</param>
/// <param name="upstreamSchemaFetcher">The fetcher needed when an upstream source selects schemas.</param>
/// <param name="manifestParser">The in-memory validator shared with manifest loading.</param>
public sealed partial class InitWriter(
    IReadOnlyDictionary<string, byte[]> shippingCopy,
    IFileSystem fileSystem,
    UpstreamSchemaFetcher? upstreamSchemaFetcher = null,
    SpecManifestParser? manifestParser = null)
{
    /// <summary>Reads the shipping copy embedded in <paramref name="assembly"/>.</summary>
    /// <param name="assembly">The assembly carrying the <c>manifest/</c>, <c>schema/</c> and <c>templates/</c> resources.</param>
    /// <returns>Each shipped file's bytes by logical name.</returns>
    public static IReadOnlyDictionary<string, byte[]> ShippingCopy(Assembly assembly) =>
        assembly.GetManifestResourceNames()
            .Where(static name => Shipped().IsMatch(name))
            .ToDictionary(static name => name, name => Read(assembly, name), StringComparer.Ordinal);

    /// <summary>Writes the newest version's manifest and each selected part that is absent under <paramref name="root"/>.</summary>
    /// <param name="root">The repository root, which must be a directory.</param>
    /// <param name="schemas">Whether the frontmatter schemas are selected.</param>
    /// <param name="templates">Whether the templates are selected.</param>
    /// <param name="cancellationToken">Cancels the run between files.</param>
    /// <param name="upstream">The HTTPS base address to record in a new manifest.</param>
    /// <param name="upstreamVersion">The upstream version label.</param>
    /// <param name="upstreamHashes">Repeated kind=hash pairs for the declared schema kinds.</param>
    /// <returns>Each file, relative to the root with <c>/</c> separators in ordinal order, and whether it was written or skipped.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
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
        var manifestPresent = fileSystem.File.Exists(fileSystem.Path.Combine([root, .. SpecManifest.RelativePath.Split('/')]));
        var selected = Newest(schemas, templates, manifestPresent).ToList();
        var hasSourceOptions = upstream is not null || upstreamVersion is not null || upstreamHashes is { Count: > 0 };
        UpstreamSchemaSource? source = null;

        if (hasSourceOptions)
        {
            if (manifestPresent)
            {
                throw new SpechtManifestException($"{SpecManifest.RelativePath}: upstream options require a new manifest.");
            }

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

            var manifestIndex = selected.FindIndex(static entry => entry.Relative == SpecManifest.RelativePath);
            var manifest = JsonNode.Parse(selected[manifestIndex].Bytes)!.AsObject();
            manifest["identifiers"]?.AsObject().Remove("epic");
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = upstream,
                ["version"] = upstreamVersion,
                ["hashes"] = hashes,
            };
            var text = manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            source = (manifestParser ?? new SpecManifestParser(new SpechtResources().DefaultManifest)).Parse(text).SpechtSchema;
            selected[manifestIndex] = (SpecManifest.RelativePath, System.Text.Encoding.UTF8.GetBytes(text + "\n"));
        }
        else if (schemas && manifestPresent)
        {
            source = SpecManifest.Load(fileSystem, root).SpechtSchema;
        }

        var preserved = new HashSet<string>(StringComparer.Ordinal);

        if (schemas && source is not null)
        {
            selected.RemoveAll(static entry => entry.Relative.StartsWith(SpecManifest.FrontmatterSchemaFolder + "/", StringComparison.Ordinal));

            foreach (var file in source.Files)
            {
                var relative = $"{SpecManifest.FrontmatterSchemaFolder}/{file.FileName}";
                var path = SpecManifest.Under(fileSystem, root, relative);
                var withinSchemas = fileSystem.Path.GetRelativePath(SpecManifest.Under(fileSystem, root, SpecManifest.FrontmatterSchemaFolder), path);

                if (fileSystem.Path.IsPathRooted(file.FileName)
                    || fileSystem.Path.IsPathRooted(withinSchemas)
                    || withinSchemas == ".."
                    || withinSchemas.StartsWith(".." + fileSystem.Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    || withinSchemas.StartsWith(".." + fileSystem.Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
                {
                    throw new SpechtManifestException($"{SpecManifest.RelativePath}: frontmatterSchemas must name files within the frontmatter schema folder.");
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

    private static byte[] Read(Assembly assembly, string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);

        return buffer.ToArray();
    }

    [GeneratedRegex(
        "^(?:manifest/v(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)/manifest\\.json|"
        + "(?<folder>schema|templates)/v(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)/(?<path>.+))$")]
    private static partial Regex Shipped();

    private IEnumerable<(string Relative, byte[] Bytes)> Newest(bool schemas, bool templates, bool manifestPresent)
    {
        var shipped = shippingCopy
            .Select(static entry => (Name: Shipped().Match(entry.Key), Bytes: entry.Value))
            .Where(static entry => entry.Name.Success)
            .Select(static entry => (
                Version: SemanticVersion.TryParse(entry.Name.Groups["version"].Value, out var version)
                    ? version
                    : throw new InvalidOperationException($"{entry.Name.Value} is embedded with an invalid schema version."),
                Folder: entry.Name.Groups["folder"].Success ? entry.Name.Groups["folder"].Value : null,
                Path: entry.Name.Groups["path"].Success ? entry.Name.Groups["path"].Value : null,
                entry.Bytes))
            .ToList();
        var newest = shipped.Max(static entry => entry.Version);

        return shipped
            .Where(entry => entry.Version == newest)
            .Where(entry => entry.Folder switch
            {
                null => true,
                "schema" => schemas && (manifestPresent || entry.Path != "frontmatter/epic.schema.json"),
                "templates" => templates,
                _ => false,
            })
            .Select(static entry => (
                Relative: entry.Folder is null
                    ? SpecManifest.RelativePath
                    : $"{SpecManifest.ConfigurationFolder}/{entry.Folder}/{entry.Path}",
                entry.Bytes))
            .OrderBy(static entry => entry.Relative, StringComparer.Ordinal)
            .Select(static entry => (entry.Relative, entry.Bytes));
    }
}
