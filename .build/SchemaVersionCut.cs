using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Specht.Build;

/// <summary>Cuts the next schema version from the newest shipped one (0001-F7 B-094 to B-100); not built.</summary>
public sealed class SchemaVersionCut(IFileSystem fileSystem, string root)
{
    /// <summary>Not built: item 0189 replaces this stub.</summary>
    public Task CutAsync(string version)
    {
        _ = (fileSystem, root, version);
        throw new NotImplementedException();
    }
}
