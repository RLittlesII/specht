using System.IO.Abstractions;

namespace Specht.Report;

/// <summary>
/// Writes the report document's text to the path a caller names, and removes it (<c>0001-F3</c> B-002, B-022 to B-024,
/// B-044, B-045; ADR-0010 § "I/O is confined"; items <c>0167</c> and <c>0036</c>). A relative path resolves from the file
/// system's working directory (B-046).
/// </summary>
/// <param name="fileSystem">The file system the document is written through.</param>
/// <implements epic="0001" feature="F1" claim="B-001" />
/// <implements epic="0001" feature="F3" claim="B-022" />
/// <implements epic="0001" feature="F3" claim="B-023" />
/// <implements epic="0001" feature="F3" claim="B-024" />
/// <implements epic="0001" feature="F3" claim="B-044" />
/// <implements epic="0001" feature="F3" claim="B-045" />
public sealed class SpechtReportStore(IFileSystem fileSystem)
{
    /// <summary>Writes <paramref name="text"/> to <paramref name="path"/>, creating its directory, and replaces a file already there.</summary>
    /// <param name="text">The serialized report document.</param>
    /// <param name="path">Where the document is written.</param>
    /// <implements epic="0001" feature="F1" claim="B-001" />
    /// <implements epic="0001" feature="F3" claim="B-022" />
    /// <implements epic="0001" feature="F3" claim="B-023" />
    /// <implements epic="0001" feature="F3" claim="B-024" />
    public void Save(string text, string path)
    {
        var directory = fileSystem.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            fileSystem.Directory.CreateDirectory(directory);
        }

        fileSystem.File.WriteAllText(path, text);
    }

    /// <summary>Removes the file at <paramref name="path"/> when there is one; creates no directory and throws for no absent file or directory.</summary>
    /// <param name="path">The path whose file is removed.</param>
    /// <implements epic="0001" feature="F3" claim="B-044" />
    /// <implements epic="0001" feature="F3" claim="B-045" />
    public void Delete(string path)
    {
        if (fileSystem.File.Exists(path))
        {
            fileSystem.File.Delete(path);
        }
    }
}
