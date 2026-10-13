using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Specht.Versioning;

namespace Specht.Build;

/// <summary>Locks the newest shipped schema version once (0001-F7 B-092, B-093).</summary>
public sealed class SchemaVersionLock(IFileSystem fileSystem, string root)
{
    /// <summary>The committed lock, outside the embedded version folders.</summary>
    public const string RelativePath = "src/tool/schema/versions.lock.json";

    /// <summary>Records the newest version unless it already has a digest.</summary>
    public async Task LockNewestAsync()
    {
        var newest = fileSystem.Directory.GetDirectories(fileSystem.Path.Combine(root, "src/tool/schema"), "v*")
            .Select(path => fileSystem.Path.GetFileName(path)[1..])
            .Select(static text => SemanticVersion.TryParse(text, out var version)
                ? version
                : throw new InvalidOperationException($"schema/v{text}/ has an invalid schema version."))
            .Max()
            .ToString();
        var lockPath = fileSystem.Path.Combine(root, RelativePath);
        var entries = fileSystem.File.Exists(lockPath)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(await fileSystem.File.ReadAllTextAsync(lockPath))!
            : new Dictionary<string, string>(StringComparer.Ordinal);
        if (entries.ContainsKey(newest))
        {
            return;
        }

        entries.Add(newest, await ComputeDigestAsync(newest));
        var temporary = $"{lockPath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await fileSystem.File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(entries, LockJson) + "\n");
            fileSystem.File.Move(temporary, lockPath, overwrite: true);
        }
        finally
        {
            fileSystem.File.Delete(temporary);
        }
    }

    /// <summary>Computes the SHA-256 over one version's ordered paths and bytes.</summary>
    public async Task<string> ComputeDigestAsync(string version)
    {
        var paths = ShippingTrees
            .Select(tree => fileSystem.Path.Combine(root, tree, $"v{version}"))
            .Where(fileSystem.Directory.Exists)
            .SelectMany(folder => fileSystem.Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            .Select(path => fileSystem.Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal);
        using var digest = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var length = new byte[8];
        foreach (var path in paths)
        {
            var name = Encoding.UTF8.GetBytes(path);
            var bytes = await fileSystem.File.ReadAllBytesAsync(fileSystem.Path.Combine(root, path));
            BinaryPrimitives.WriteInt64BigEndian(length, name.Length);
            digest.AppendData(length);
            digest.AppendData(name);
            BinaryPrimitives.WriteInt64BigEndian(length, bytes.Length);
            digest.AppendData(length);
            digest.AppendData(bytes);
        }

        return Convert.ToHexStringLower(digest.GetHashAndReset());
    }

    private static readonly JsonSerializerOptions LockJson = new() { WriteIndented = true };

    private static readonly string[] ShippingTrees = ["src/tool/schema", "src/tool/manifest", "src/tool/templates", "docs/rules"];
}
