using System.IO.Abstractions.TestingHelpers;

namespace Specht.Tests.Discovery;

/// <summary>The directory that records each listing of a <see cref="RecordingFileSystem"/>.</summary>
internal sealed class RecordingDirectory(IMockFileDataAccessor files, string currentDirectory) : MockDirectory(files, currentDirectory)
{
    public List<RecordedListing> Recording { get; } = [];

    public override IEnumerable<string> EnumerateFiles(string path) => Record(path, base.EnumerateFiles(path));

    public override IEnumerable<string> EnumerateFiles(string path, string searchPattern) =>
        Record(path, base.EnumerateFiles(path, searchPattern));

    public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) =>
        Record(path, base.EnumerateFiles(path, searchPattern, searchOption));

    public override IEnumerable<string> EnumerateFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
        Record(path, base.EnumerateFiles(path, searchPattern, enumerationOptions));

    public override IEnumerable<string> EnumerateDirectories(string path) => Record(path, base.EnumerateDirectories(path));

    public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern) =>
        Record(path, base.EnumerateDirectories(path, searchPattern));

    public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption) =>
        Record(path, base.EnumerateDirectories(path, searchPattern, searchOption));

    public override IEnumerable<string> EnumerateDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
        Record(path, base.EnumerateDirectories(path, searchPattern, enumerationOptions));

    public override IEnumerable<string> EnumerateFileSystemEntries(string path) => Record(path, base.EnumerateFileSystemEntries(path));

    public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern) =>
        Record(path, base.EnumerateFileSystemEntries(path, searchPattern));

    public override IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption) =>
        Record(path, base.EnumerateFileSystemEntries(path, searchPattern, searchOption));

    public override IEnumerable<string> EnumerateFileSystemEntries(
        string path, string searchPattern, EnumerationOptions enumerationOptions) =>
        Record(path, base.EnumerateFileSystemEntries(path, searchPattern, enumerationOptions));

    public override string[] GetFiles(string path) => Record(path, base.GetFiles(path)).ToArray();

    public override string[] GetFiles(string path, string searchPattern) => Record(path, base.GetFiles(path, searchPattern)).ToArray();

    public override string[] GetFiles(string path, string searchPattern, SearchOption searchOption) =>
        Record(path, base.GetFiles(path, searchPattern, searchOption)).ToArray();

    public override string[] GetFiles(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
        Record(path, base.GetFiles(path, searchPattern, enumerationOptions)).ToArray();

    public override string[] GetDirectories(string path) => Record(path, base.GetDirectories(path)).ToArray();

    public override string[] GetDirectories(string path, string searchPattern) =>
        Record(path, base.GetDirectories(path, searchPattern)).ToArray();

    public override string[] GetDirectories(string path, string searchPattern, SearchOption searchOption) =>
        Record(path, base.GetDirectories(path, searchPattern, searchOption)).ToArray();

    public override string[] GetDirectories(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
        Record(path, base.GetDirectories(path, searchPattern, enumerationOptions)).ToArray();

    public override string[] GetFileSystemEntries(string path) => Record(path, base.GetFileSystemEntries(path)).ToArray();

    public override string[] GetFileSystemEntries(string path, string searchPattern) =>
        Record(path, base.GetFileSystemEntries(path, searchPattern)).ToArray();

    public override string[] GetFileSystemEntries(string path, string searchPattern, SearchOption searchOption) =>
        Record(path, base.GetFileSystemEntries(path, searchPattern, searchOption)).ToArray();

    public override string[] GetFileSystemEntries(string path, string searchPattern, EnumerationOptions enumerationOptions) =>
        Record(path, base.GetFileSystemEntries(path, searchPattern, enumerationOptions)).ToArray();

    private IReadOnlyList<string> Record(string path, IEnumerable<string> listed)
    {
        var returned = listed.ToList();
        Recording.Add(new RecordedListing(path, returned));
        return returned;
    }
}
