using System.Text.Json;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>Lists and pins schema versions through the existing manifest services.</summary>
/// <implements epic="0001" feature="F7" claim="B-043" />
public sealed class SchemaVersionManager(SpecManifestStore store, SpecManifestParser parser, ManifestPinEditor editor, SchemaVersions versions)
{
    /// <summary>Lists shipped versions with the optional manifest's declared or default pin.</summary>
    /// <implements epic="0001" feature="F7" claim="B-043" />
    public (IReadOnlyList<SemanticVersion> Versions, SemanticVersion? Pinned) List(string root)
    {
        var bytes = ReadManifest(root, required: false);
        SemanticVersion? pin = null;
        if (bytes is not null)
        {
            try
            {
                pin = parser.PinnedVersion(parser.Parse(editor.Decode(bytes))).Number;
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or ArgumentException)
            {
                throw new SpechtManifestUnreadableException(exception);
            }
        }
        return (Shipped(), pin);
    }

    /// <summary>Validates the pin input before returning choices for the host's selector.</summary>
    /// <implements epic="0001" feature="F7" claim="B-043" />
    public IReadOnlyList<SemanticVersion> Choices(string root)
    {
        editor.Validate(ReadManifest(root, required: true)!);
        return Shipped();
    }

    /// <summary>Changes only the pin after root, settings and target validation.</summary>
    /// <implements epic="0001" feature="F7" claim="B-043" />
    public async Task<SemanticVersion> Pin(string root, string version, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bytes = ReadManifest(root, required: true)!;
        editor.Validate(bytes);
        if (!SemanticVersion.TryParse(version, out var target) || versions.Find(target) is null)
        {
            throw new SchemaVersionNotFoundException(version, Shipped());
        }
        var changed = editor.Change(bytes, target);
        if (!bytes.AsSpan().SequenceEqual(changed))
        {
            try
            {
                await store.WriteManifest(root, changed, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: the manifest could not be replaced; check write access.");
            }
        }
        return target;
    }

    /// <implements epic="0001" feature="F7" claim="B-043" />
    private SemanticVersion[] Shipped() => versions.Versions.Select(static version => version.Number).ToArray();

    /// <implements epic="0001" feature="F7" claim="B-043" />
    private byte[]? ReadManifest(string root, bool required)
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
}
