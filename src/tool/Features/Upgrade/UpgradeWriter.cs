using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Specht.Manifest;
using Specht.Model;
using Specht.Versioning;

namespace Specht.Tool.Features.Upgrade;

/// <summary>
/// Brings the frontmatter schemas and templates present under a root to the bytes the pinned version ships (<c>0001-F7</c>
/// B-005), adds the keys the pinned version defines and the manifest lacks (B-016), leaves a root that already holds them as
/// it was (B-006) and writes only through the store's replace (B-007, C-2).
/// </summary>
/// <param name="loader">Reads and validates the manifest as the check does.</param>
/// <param name="parser">Selects the pinned version.</param>
/// <param name="shipped">The embedded files.</param>
/// <param name="store">Reads and replaces the configuration files.</param>
/// <param name="keys">Adds the pinned version's missing keys to the manifest's bytes.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
/// <implements epic="0001" feature="F7" claim="B-016" />
public sealed class UpgradeWriter(
    SpecSchemasLoader loader,
    SpecManifestParser parser,
    ShippedFiles shipped,
    SpecManifestStore store,
    ManifestKeyEditor keys)
{
    /// <summary>
    /// Rewrites each present file that differs from the pinned version's, and the manifest when it lacks a key that version
    /// defines, and none other.
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="cancellationToken">Cancels the run between files.</param>
    /// <returns>The pin and the rewritten files.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">The root holds no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest cannot be parsed.</exception>
    /// <exception cref="SpechtManifestException">The pin is not shipped, or a write failed.</exception>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    /// <implements epic="0001" feature="F7" claim="B-016" />
    /// <implements epic="0001" feature="F7" claim="B-059" />
    public async Task<UpgradeResult> Upgrade(string root, CancellationToken cancellationToken)
    {
        var structure = loader.LoadManifest(root);
        var pinned = parser.PinnedVersion(structure).Number;
        var frontmatterFromRoot = structure.SchemaSource == SchemaSourceSelection.Disk || structure.SpechtSchema is not null;
        var plan = shipped.At(pinned)
            .Where(file => !frontmatterFromRoot
                           || !file.Relative.StartsWith(SpecManifestStore.FrontmatterSchemaFolder + "/", StringComparison.Ordinal))
            .Select(file => (File: file, Present: store.ReadConfigurationFile(root, file.Relative)))
            .Where(static entry => entry.Present is not null)
            .Select(entry => (entry.Present, Planned: new ShippedFile(
                entry.File.Relative,
                entry.File.Relative == SpecManifestStore.RelativePath ? keys.AddMissing(entry.Present!, entry.File.Bytes) : entry.File.Bytes)))
            .Where(static entry => !entry.Present.AsSpan().SequenceEqual(entry.Planned.Bytes))
            .Select(static entry => entry.Planned)
            .ToList();

        foreach (var file in plan)
        {
            try
            {
                await store.ReplaceConfigurationFile(root, file.Relative, file.Bytes, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                throw new SpechtManifestException($"{file.Relative}: could not be replaced.");
            }
        }

        return new UpgradeResult(pinned, plan.Select(static file => file.Relative).ToList());
    }
}
