using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Specht.Tests.Discovery;

/// <summary>
/// A <see cref="MockFileSystem"/> whose directory records, through every <c>Enumerate*</c> and <c>Get*</c> listing overload,
/// the path each listing is asked for and every path it returns (<c>0001-F6</c> B-005, B-006, C-1).
/// </summary>
internal sealed class RecordingFileSystem : MockFileSystem
{
    public RecordingFileSystem(IDictionary<string, MockFileData> files)
        : base(files) =>
        _directory = new RecordingDirectory(this, base.Directory.GetCurrentDirectory());

    public override IDirectory Directory => _directory ?? base.Directory;

    public List<RecordedListing> Recording => _directory?.Recording ?? [];

    private readonly RecordingDirectory? _directory;
}
