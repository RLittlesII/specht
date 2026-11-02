using System.Text;

namespace Specht.Tests.Tree;

/// <summary>
/// One text under each encoding <see cref="File.ReadAllText(string)"/> tells apart: a UTF-8, UTF-16 little- and big-endian
/// or UTF-32 little-endian byte-order mark, and none, which is UTF-8 (item <c>0166</c> criterion 10).
/// </summary>
public sealed class EncodedTexts : TheoryData<string, byte[], string>
{
    /// <summary>Initializes a new instance of the <see cref="EncodedTexts"/> class.</summary>
    public EncodedTexts()
    {
        Add("a UTF-8 byte-order mark", Encoded(new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)), Text);
        Add("a UTF-16 little-endian byte-order mark", Encoded(new UnicodeEncoding(bigEndian: false, byteOrderMark: true)), Text);
        Add("a UTF-16 big-endian byte-order mark", Encoded(new UnicodeEncoding(bigEndian: true, byteOrderMark: true)), Text);
        Add("a UTF-32 little-endian byte-order mark", Encoded(new UTF32Encoding(bigEndian: false, byteOrderMark: true)), Text);
        Add("no byte-order mark", Encoded(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)), Text);
    }

    private static byte[] Encoded(Encoding encoding) => [.. encoding.GetPreamble(), .. encoding.GetBytes(Text)];

    private const string Text = "---\ntitle: \"Größe\"\n---\n\n# Größe\n";
}
