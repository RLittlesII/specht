namespace Specht.Tests.Manifest;

/// <summary>The manifest encodings the reader accepts, by name, and whether the bytes open with that encoding's BOM.</summary>
public sealed class ManifestKeyEncodings : TheoryData<string, bool>
{
    public ManifestKeyEncodings()
    {
        Add("utf-8", false);
        Add("utf-8", true);
        Add("utf-16", true);
        Add("utf-16BE", true);
        Add("utf-32", true);
        Add("utf-32BE", true);
    }
}
