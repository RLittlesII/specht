using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specht.Build;

/// <summary>Cuts the next schema version from the newest shipped one (0001-F7 B-094 to B-100).</summary>
public sealed class SchemaVersionCut(IFileSystem fileSystem, string root)
{
    /// <summary>Copies the newest version's folders to the named version, refusing before any write.</summary>
    public async Task CutAsync(string version)
    {
        var versionLock = new SchemaVersionLock(fileSystem, root);
        await versionLock.RequireAllLockedAsync();
        if (!SchemaVersionLock.TryParseVersion(version, out var named))
        {
            throw new InvalidOperationException($"The cut version '{version}' is not major.minor.patch.");
        }

        var newestVersion = versionLock.ReadVersions().Max()!;
        var newest = newestVersion.ToString(3);
        if (named <= newestVersion)
        {
            throw new InvalidOperationException(
                $"The cut version {version} is not higher than the newest shipped schema version {newest}.");
        }

        var trees = SchemaVersionLock.ShippingTrees
            .Where(tree => fileSystem.Directory.Exists(fileSystem.Path.Combine(root, tree, $"v{newest}")))
            .ToArray();
        var existing = trees.FirstOrDefault(tree => fileSystem.Directory.Exists(fileSystem.Path.Combine(root, tree, $"v{version}")));
        if (existing is not null)
        {
            throw new InvalidOperationException($"{existing}/v{version}/ already exists.");
        }

        var plan = new List<(string Tree, string Path, byte[] Bytes)>();
        foreach (var tree in trees)
        {
            var folder = fileSystem.Path.Combine(root, tree, $"v{newest}");
            var paths = fileSystem.Directory.GetFiles(folder, "*", SearchOption.AllDirectories)
                .Select(path => fileSystem.Path.GetRelativePath(folder, path).Replace('\\', '/'))
                .Order(StringComparer.Ordinal);
            foreach (var path in paths)
            {
                var bytes = await fileSystem.File.ReadAllBytesAsync(fileSystem.Path.Combine(folder, path));
                plan.Add((tree, path, Rewrite($"{tree}/v{newest}/{path}", tree, path, bytes, newest, version)));
            }
        }

        await WriteAsync(trees, plan, version);
    }

    private static byte[] Rewrite(string source, string tree, string path, byte[] bytes, string newest, string version)
    {
        if (string.Equals(tree, "src/tool/schema", StringComparison.Ordinal)
            && path.StartsWith("frontmatter/", StringComparison.Ordinal)
            && !path["frontmatter/".Length..].Contains('/', StringComparison.Ordinal)
            && path.EndsWith(".schema.json", StringComparison.Ordinal))
        {
            return Splice(
                bytes,
                Encoding.ASCII.GetBytes($"{SchemaAddress}v{newest}/frontmatter/"),
                Encoding.ASCII.GetBytes($"{SchemaAddress}v{version}/frontmatter/"),
                $"{source} does not state its $id exactly once.");
        }

        if (string.Equals(tree, "docs/rules", StringComparison.Ordinal)
            && !path.Contains('/', StringComparison.Ordinal)
            && path.EndsWith(".md", StringComparison.Ordinal))
        {
            var refusal = $"{source} does not give its schema version in exactly one row.";
            var rows = Enumerable.Range(0, bytes.Length)
                .Where(index => (index == 0 || bytes[index - 1] == (byte)'\n') && bytes.AsSpan(index).StartsWith(SchemaVersionRow))
                .ToArray();
            if (rows.Length != 1)
            {
                throw new InvalidOperationException(refusal);
            }

            var start = rows[0];
            var length = bytes.AsSpan(start).IndexOf((byte)'\n');
            var end = length < 0 ? bytes.Length : start + length;
            return
            [
                .. bytes.AsSpan(..start),
                .. Splice(bytes[start..end], Encoding.ASCII.GetBytes($" {newest} "), Encoding.ASCII.GetBytes($" {version} "), refusal),
                .. bytes.AsSpan(end..),
            ];
        }

        return bytes;
    }

    private static byte[] Splice(byte[] bytes, byte[] token, byte[] replacement, string refusal)
    {
        var at = bytes.AsSpan().IndexOf(token);
        if (at < 0 || bytes.AsSpan(at + 1).IndexOf(token) >= 0)
        {
            throw new InvalidOperationException(refusal);
        }

        return [.. bytes.AsSpan(..at), .. replacement, .. bytes.AsSpan((at + token.Length)..)];
    }

    private async Task WriteAsync(string[] trees, List<(string Tree, string Path, byte[] Bytes)> plan, string version)
    {
        var run = Guid.NewGuid().ToString("N");
        var staged = trees
            .Select(tree => (
                Tree: tree,
                Staging: fileSystem.Path.Combine(root, tree, $".v{version}.{run}.tmp"),
                Target: fileSystem.Path.Combine(root, tree, $"v{version}")))
            .ToArray();
        var moved = new List<string>();
        try
        {
            foreach (var (tree, staging, _) in staged)
            {
                fileSystem.Directory.CreateDirectory(staging);
                foreach (var (_, path, bytes) in plan.Where(file => string.Equals(file.Tree, tree, StringComparison.Ordinal)))
                {
                    var fullPath = fileSystem.Path.Combine(staging, path);
                    fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(fullPath)!);
                    await fileSystem.File.WriteAllBytesAsync(fullPath, bytes);
                }
            }

            foreach (var (_, staging, target) in staged)
            {
                fileSystem.Directory.Move(staging, target);
                moved.Add(target);
            }
        }
        catch
        {
            foreach (var target in moved)
            {
                fileSystem.Directory.Delete(target, recursive: true);
            }

            throw;
        }
        finally
        {
            foreach (var (_, staging, _) in staged.Where(entry => fileSystem.Directory.Exists(entry.Staging)))
            {
                fileSystem.Directory.Delete(staging, recursive: true);
            }
        }
    }

    private const string SchemaAddress = "https://github.com/rlittlesii/specht/schema/";

    private static readonly byte[] SchemaVersionRow = "| Schema version "u8.ToArray();
}
