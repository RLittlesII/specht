using System.Text.Json;

namespace Specht.Manifest;

/// <summary>Validates the current manifest and atomically sets one pinned rule's explicit level.</summary>
/// <implements epic="0001" feature="F11" claim="B-001" />
public sealed class RuleLevelSetter(SpecManifestStore store, SpecManifestParser parser, ManifestRuleEditor editor, ManifestPinEditor encoding)
{
    /// <summary>Changes one rule only after validating the root, complete manifest, pinned id and exact level.</summary>
    /// <implements epic="0001" feature="F11" claim="B-001" />
    /// <implements epic="0001" feature="F11" claim="B-006" />
    /// <implements epic="0001" feature="F11" claim="B-007" />
    /// <implements epic="0001" feature="F11" claim="B-008" />
    /// <implements epic="0001" feature="F11" claim="B-009" />
    /// <implements epic="0001" feature="F11" claim="B-010" />
    /// <implements epic="0001" feature="F11" claim="B-011" />
    /// <implements epic="0001" feature="F11" claim="B-012" />
    public async Task Set(string root, string id, string level, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!store.RootExists(root))
        {
            throw new SpechtRootNotFoundException();
        }
        if (!store.ManifestExists(root))
        {
            throw new SpechtManifestNotFoundException(store.RetiredManifestExists(root));
        }
        byte[] bytes;
        try
        {
            bytes = store.ReadManifestBytes(root);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
        SpecStructure structure;
        try
        {
            structure = parser.Parse(encoding.Decode(bytes));
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException or FormatException or ArgumentException)
        {
            throw new SpechtManifestUnreadableException(exception);
        }
        var version = parser.PinnedVersion(structure);
        var faults = new List<string>();
        if (!version.RuleIds.Contains(id))
        {
            faults.Add($"Rule '{id}' is not held by schemaVersion {version.Number}.");
        }
        if (level is not ("error" or "warning" or "off"))
        {
            faults.Add($"Level '{level}' is not one of error, warning or off.");
        }
        if (faults.Count > 0)
        {
            throw new RuleLevelNotFoundException(string.Join(" ", faults));
        }
        var changed = editor.Change(bytes, id, level);
        if (bytes.AsSpan().SequenceEqual(changed))
        {
            return;
        }
        try
        {
            await store.WriteManifest(root, changed, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new SpechtManifestException($"{SpecManifestStore.RelativePath}: the manifest could not be replaced; check write access.");
        }
    }
}
