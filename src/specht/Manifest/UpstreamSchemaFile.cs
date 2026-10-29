using System.Security.Cryptography;

namespace Specht.Manifest;

/// <summary>One frontmatter schema an upstream schema source is fetched for.</summary>
/// <param name="Kind">The kind, a <c>frontmatterSchemas</c> key.</param>
/// <param name="FileName">The file name the manifest gives the kind (<c>0001-F5</c> B-008).</param>
/// <param name="Address">The source's location followed by <paramref name="FileName"/> (<c>0001-F7</c> B-074).</param>
/// <param name="Hash">The recorded content hash, <c>sha256:</c> and 64 lowercase hexadecimal digits.</param>
/// <implements epic="0001" feature="F7" claim="B-068" />
public sealed record UpstreamSchemaFile(string Kind, string FileName, string Address, string Hash)
{
    /// <summary>Whether the SHA-256 of <paramref name="bytes"/> is the recorded hash (<c>0001-F7</c> C-9).</summary>
    /// <param name="bytes">The bytes as read or served, without decoding.</param>
    /// <returns><see langword="true"/> when <c>sha256:</c> and the lowercase hexadecimal digest equal <see cref="Hash"/> by ordinal.</returns>
    /// <implements epic="0001" feature="F7" claim="B-033" />
    public bool Matches(byte[] bytes) =>
        string.Equals("sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes)), Hash, StringComparison.Ordinal);
}
