using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;

namespace Specht.Tool.Features.Init;

/// <summary>
/// Writes the newest shipped manifest, and the selected schemas and templates, under <c>&lt;root&gt;/.specht/</c>
/// (<c>0001-F4</c> B-029 to B-033), leaving every file that exists as it was (C-1).
/// </summary>
/// <param name="shippingCopy">
/// The embedded files by logical name: <c>manifest/v&lt;n&gt;/manifest.json</c>, <c>schema/v&lt;n&gt;/frontmatter/&lt;file&gt;</c> or
/// <c>templates/v&lt;n&gt;/&lt;file&gt;</c>.
/// </param>
/// <param name="fileSystem">The file system the root is written through.</param>
public sealed partial class InitWriter(IReadOnlyDictionary<string, byte[]> shippingCopy, IFileSystem fileSystem)
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
    /// <returns>Each file, relative to the root with <c>/</c> separators in ordinal order, and whether it was written or skipped.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    public async Task<IReadOnlyList<(string Path, bool Written)>> Write(
        string root,
        bool schemas,
        bool templates,
        CancellationToken cancellationToken)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        var listing = new List<(string Path, bool Written)>();
        var manifestPresent = fileSystem.File.Exists(fileSystem.Path.Combine([root, .. SpecManifest.RelativePath.Split('/')]));

        foreach (var (relative, bytes) in Newest(schemas, templates, manifestPresent))
        {
            var path = fileSystem.Path.Combine([root, .. relative.Split('/')]);

            if (fileSystem.File.Exists(path))
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

    [GeneratedRegex("^(?:manifest/v(?<version>[0-9]+)/manifest\\.json|(?<folder>schema|templates)/v(?<version>[0-9]+)/(?<path>.+))$")]
    private static partial Regex Shipped();

    private IEnumerable<(string Relative, byte[] Bytes)> Newest(bool schemas, bool templates, bool manifestPresent)
    {
        var shipped = shippingCopy
            .Select(static entry => (Name: Shipped().Match(entry.Key), Bytes: entry.Value))
            .Where(static entry => entry.Name.Success)
            .Select(static entry => (
                Version: int.Parse(entry.Name.Groups["version"].Value, CultureInfo.InvariantCulture),
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
