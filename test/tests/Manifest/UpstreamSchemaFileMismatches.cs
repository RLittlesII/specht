using System.Text;

namespace Specht.Tests.Manifest;

/// <summary>
/// Bytes an upstream schema source records, and bytes other than them, each with why they differ (<c>0001-F7</c> B-033,
/// C-9): differing content, and the same text after a UTF-8 byte order mark, which differs only if the bytes are compared
/// without decoding.
/// </summary>
public sealed class UpstreamSchemaFileMismatches : TheoryData<string, byte[], byte[]>
{
    public UpstreamSchemaFileMismatches()
    {
        var recorded = Encoding.UTF8.GetBytes("""{ "title": "the item schema that was fetched" }""");
        Add("the content differs", recorded, Encoding.UTF8.GetBytes("""{ "title": "a schema edited by hand" }"""));
        Add("the same text follows a byte order mark", recorded, [.. Encoding.UTF8.GetPreamble(), .. recorded]);
    }
}
