using System.Security.Cryptography;

namespace Specht.Manifest;

/// <summary>One template an upstream schema source supplies (<c>0001-F7</c> B-125, decision 0016).</summary>
/// <param name="FileName">The template's file name under <c>.specht/templates/</c>, the <c>templates</c> member's name.</param>
/// <param name="Address">The source's location followed by <paramref name="FileName"/> (<c>0001-F7</c> B-131).</param>
/// <param name="Hash">The recorded content hash, <c>sha256:</c> and 64 lowercase hexadecimal digits.</param>
/// <implements epic="0001" feature="F7" claim="B-125" />
public sealed record UpstreamTemplateFile(string FileName, string Address, string Hash)
{
    /// <summary>Whether the SHA-256 of <paramref name="bytes"/> is the recorded hash (<c>0001-F7</c> C-9).</summary>
    /// <param name="bytes">The bytes as read or served, without decoding.</param>
    /// <returns><see langword="true"/> when <c>sha256:</c> and the lowercase hexadecimal digest equal <see cref="Hash"/> by ordinal.</returns>
    /// <implements epic="0001" feature="F7" claim="B-130" />
    /// <implements epic="0001" feature="F7" claim="B-134" />
    /// <implements epic="0001" feature="F7" claim="B-136" />
    public bool Matches(byte[] bytes) =>
        string.Equals("sha256:" + Convert.ToHexStringLower(SHA256.HashData(bytes)), Hash, StringComparison.Ordinal);
}
