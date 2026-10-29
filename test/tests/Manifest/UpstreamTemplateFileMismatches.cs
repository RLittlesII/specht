using System.Text;

namespace Specht.Tests.Manifest;

/// <summary>
/// Bytes an upstream schema source records for a template, and bytes other than them, each with why they differ
/// (<c>0001-F7</c> B-130, B-136, C-9): differing content, and the same text after a UTF-8 byte order mark, which differs
/// only if the bytes are compared without decoding.
/// </summary>
public sealed class UpstreamTemplateFileMismatches : TheoryData<string, byte[], byte[]>
{
    public UpstreamTemplateFileMismatches()
    {
        var recorded = Encoding.UTF8.GetBytes("# The feature template that was fetched\n");
        Add("the content differs", recorded, Encoding.UTF8.GetBytes("# A feature template edited by hand\n"));
        Add("the same text follows a byte order mark", recorded, [.. Encoding.UTF8.GetPreamble(), .. recorded]);
    }
}
