using System.Security.Cryptography;
using System.Text;

namespace Specht.Manifest;

/// <summary>Verifies the bytes of an upstream copy against its recorded hashes (0001-F7 B-077 to B-081).</summary>
public sealed class UpstreamSchemaCopy
{
    /// <summary>Verifies every fetched file and returns its schema text by kind.</summary>
    /// <param name="source">The source and hashes the manifest records.</param>
    /// <param name="files">The bytes read for each fetched kind, or null for an absent file.</param>
    /// <returns>The verified text of each fetched kind.</returns>
    /// <exception cref="SpechtManifestException">A file is absent or differs from its hash.</exception>
    public IReadOnlyDictionary<string, string> Verify(UpstreamSchemaSource source, IReadOnlyDictionary<string, byte[]?> files)
    {
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        var faults = new List<string>();
        foreach (var file in source.Files)
        {
            var path = SpecManifest.FrontmatterSchemaFolder + "/" + file.FileName;
            var bytes = files.GetValueOrDefault(file.Kind);
            if (bytes is null)
            {
                faults.Add($"'{path}' is absent; run specht init to fetch it");
            }
            else if ("sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes)) != file.Hash)
            {
                faults.Add($"'{path}' does not match its recorded SHA-256 hash");
            }
            else
            {
                texts.Add(file.Kind, Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'));
            }
        }

        if (faults.Count > 0)
        {
            throw new SpechtManifestException($"{SpecManifest.RelativePath}: {string.Join("; ", faults)}.");
        }

        return texts;
    }
}
