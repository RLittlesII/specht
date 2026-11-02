using System.Runtime.InteropServices;
using System.Text;

namespace Specht.Tree;

/// <summary>One file of a <see cref="SpechtTree"/>.</summary>
/// <param name="RelativePath">The file's path under the tree's root, its segments separated by <c>/</c>.</param>
/// <param name="Content">The file's bytes.</param>
/// <implements epic="0109" feature="F1" claim="B-016" />
public sealed record SpechtTreeFile(string RelativePath, ReadOnlyMemory<byte> Content)
{
    /// <summary>
    /// The file's text, decoded as <see cref="File.ReadAllText(string)"/> decodes a file: a UTF-8, UTF-16 or UTF-32
    /// byte-order mark selects its encoding and is dropped, and any other content is UTF-8.
    /// </summary>
    /// <returns>The decoded text.</returns>
    /// <implements epic="0001" feature="F1" claim="B-004" />
    public string Text()
    {
        var bytes = MemoryMarshal.TryGetArray(Content, out var segment)
            ? new MemoryStream(segment.Array!, segment.Offset, segment.Count, writable: false)
            : new MemoryStream(Content.ToArray(), writable: false);
        using var reader = new StreamReader(bytes, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
