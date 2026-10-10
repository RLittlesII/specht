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

namespace Specht.Tool.Features.Init;

/// <summary>
/// Writes the newest shipped version of the eight files into <c>&lt;root&gt;/.spec/schema/</c> and
/// <c>&lt;root&gt;/.spec/templates/</c> (<c>0001-F4</c> B-001, B-003, B-008), leaving every file that exists as it was (C-1).
/// </summary>
/// <param name="shippingCopy">The embedded files by logical name, <c>schema/v&lt;n&gt;/&lt;file&gt;</c> or <c>templates/v&lt;n&gt;/&lt;file&gt;</c>.</param>
/// <param name="fileSystem">The file system the root is written through.</param>
public sealed partial class InitWriter(IReadOnlyDictionary<string, byte[]> shippingCopy, IFileSystem fileSystem)
{
    /// <summary>Reads the shipping copy embedded in <paramref name="assembly"/>.</summary>
    /// <param name="assembly">The assembly carrying the <c>schema/v&lt;n&gt;/</c> and <c>templates/v&lt;n&gt;/</c> resources.</param>
    /// <returns>Each shipped file's bytes by logical name.</returns>
    public static IReadOnlyDictionary<string, byte[]> ShippingCopy(Assembly assembly) =>
        assembly.GetManifestResourceNames()
            .Where(static name => Shipped().IsMatch(name))
            .ToDictionary(static name => name, name => Read(assembly, name), StringComparer.Ordinal);

    /// <summary>Writes each of the newest version's files that is absent under <paramref name="root"/>.</summary>
    /// <param name="root">The repository root, which must be a directory.</param>
    /// <param name="cancellationToken">Cancels the run between files.</param>
    /// <returns>Each file, relative to the root with <c>/</c> separators in ordinal order, and whether it was written or skipped.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    public async Task<IReadOnlyList<(string Path, bool Written)>> Write(string root, CancellationToken cancellationToken)
    {
        if (!fileSystem.Directory.Exists(root))
        {
            throw new SpechtRootNotFoundException();
        }

        var listing = new List<(string Path, bool Written)>();

        foreach (var (relative, bytes) in Newest())
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

    [GeneratedRegex("^(?<folder>schema|templates)/v(?<version>[0-9]+)/(?<file>[^/]+)$")]
    private static partial Regex Shipped();

    private IEnumerable<(string Relative, byte[] Bytes)> Newest()
    {
        var shipped = shippingCopy
            .Select(static entry => (Name: Shipped().Match(entry.Key), Bytes: entry.Value))
            .Where(static entry => entry.Name.Success)
            .Select(static entry => (
                Version: int.Parse(entry.Name.Groups["version"].Value, CultureInfo.InvariantCulture),
                Relative: $".spec/{entry.Name.Groups["folder"].Value}/{entry.Name.Groups["file"].Value}",
                entry.Bytes))
            .ToList();
        var newest = shipped.Max(static entry => entry.Version);

        return shipped
            .Where(entry => entry.Version == newest)
            .OrderBy(static entry => entry.Relative, StringComparer.Ordinal)
            .Select(static entry => (entry.Relative, entry.Bytes));
    }
}
