using System.IO.Abstractions;

namespace Specht.Report;

/// <summary>Writes the report document's text to the path a caller names (<c>0001-F3</c> B-002; ADR-0010 § "I/O is confined"; item <c>0167</c>).</summary>
/// <param name="fileSystem">The file system the document is written through.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SpechtReportStore(IFileSystem fileSystem)
{
    /// <summary>Writes <paramref name="text"/> to <paramref name="path"/>, creating its directory.</summary>
    /// <param name="text">The serialized report document.</param>
    /// <param name="path">Where the document is written.</param>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    public void Save(string text, string path)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);
        fileSystem.File.WriteAllText(path, text);
    }
}
