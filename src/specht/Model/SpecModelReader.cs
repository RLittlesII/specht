using Specht.Manifest;

namespace Specht.Model;

/// <summary>Reads command input at the file-system edge through the existing manifest store (ADR-0010).</summary>
/// <param name="store">The manifest store.</param>
public sealed class SpecModelReader(SpecManifestStore store)
{
    /// <summary>Reads the manifest bytes, or null for an optional manifest that is absent.</summary>
    public byte[]? ReadManifest(string root, bool required)
    {
        if (!store.RootExists(root))
        {
            throw new SpechtRootNotFoundException();
        }
        if (!store.ManifestExists(root))
        {
            return required ? throw new SpechtManifestNotFoundException(store.RetiredManifestExists(root)) : null;
        }
        try
        {
            return store.ReadManifestBytes(root);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
    }

    /// <summary>Writes the in-memory edit through the existing manifest store.</summary>
    public async Task WriteManifest(string root, byte[] bytes, CancellationToken cancellationToken)
    {
        try
        {
            await store.WriteManifest(root, bytes, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: the manifest could not be replaced; check write access.");
        }
    }
}
