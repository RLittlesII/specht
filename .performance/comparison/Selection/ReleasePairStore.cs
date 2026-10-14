using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Persists a versioned pair without replacing an existing selection.</summary>
public sealed class ReleasePairStore(IFileSystem fileSystem, string directory)
{
    public async Task<ReleasePair> GetOrCreateAsync(ReleaseRecord current,
        Func<CancellationToken, Task<ReleasePair>> create, CancellationToken cancellationToken = default)
    {
        var path = fileSystem.Path.Combine(directory, "comparison-pair-v1.json");
        if (fileSystem.File.Exists(path)) return await ReadAsync(path, current, cancellationToken);
        var pair = await create(cancellationToken);
        ComparisonJson.ValidatePair(pair, current);
        fileSystem.Directory.CreateDirectory(directory);
        var temporary = fileSystem.Path.Combine(directory, fileSystem.Path.GetRandomFileName());
        try
        {
            await using (var stream = fileSystem.FileStream.New(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var bytes = Encoding.UTF8.GetBytes(ComparisonJson.Write(pair));
                await stream.WriteAsync(bytes, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                fileSystem.File.Move(temporary, path, false);
            }
            catch (IOException) when (fileSystem.File.Exists(path)) { }
            return await ReadAsync(path, current, cancellationToken);
        }
        finally
        {
            if (fileSystem.File.Exists(temporary)) fileSystem.File.Delete(temporary);
        }
    }

    private async Task<ReleasePair> ReadAsync(string path, ReleaseRecord current, CancellationToken cancellationToken)
    {
        if ((fileSystem.File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new InvalidDataException("Saved pair cannot be a symbolic link.");
        try
        {
            return ComparisonJson.ReadPair(await fileSystem.File.ReadAllTextAsync(path, cancellationToken), current);
        }
        catch (IOException)
        {
            throw new InvalidDataException("Saved pair is unreadable; selection must not be replaced.");
        }
    }
}
