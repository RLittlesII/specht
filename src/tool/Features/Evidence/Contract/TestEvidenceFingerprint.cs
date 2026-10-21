using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Specht.Tool.Features.Evidence.Contract;

/// <implements epic="0201" feature="F3" claim="B-009" />
public sealed class TestEvidenceFingerprint
{
    /// <implements epic="0201" feature="F3" claim="B-009" />
    public string Compute(IEnumerable<TestEvidenceFingerprintSource> sources)
    {
        using var sha = SHA256.Create();
        return ComputeDigests(sources.Select(source => new TestEvidenceInput(source.Path,
            Hex(sha.ComputeHash(source.Content)), source.Kind)));
    }

    /// <implements epic="0201" feature="F3" claim="B-009" />
    public string ComputeDigests(IEnumerable<TestEvidenceInput> inputs)
    {
        using var canonical = new MemoryStream();
        foreach (var input in inputs.OrderBy(static item => item.Path, StringComparer.Ordinal)
                     .ThenBy(static item => item.Kind, StringComparer.Ordinal))
        {
            Append(canonical, input.Path);
            Append(canonical, input.Kind);
            Append(canonical, input.Sha256);
        }

        using var sha = SHA256.Create();
        return Hex(sha.ComputeHash(canonical.ToArray()));
    }

    /// <implements epic="0201" feature="F3" claim="B-009" />
    private static void Append(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        var length = (uint)bytes.Length;
        stream.WriteByte((byte)(length >> 24));
        stream.WriteByte((byte)(length >> 16));
        stream.WriteByte((byte)(length >> 8));
        stream.WriteByte((byte)length);
        stream.Write(bytes, 0, bytes.Length);
    }

    /// <implements epic="0201" feature="F3" claim="B-009" />
    private static string Hex(byte[] bytes) => string.Concat(bytes.Select(static value => value.ToString("x2")));
}
