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
/// B-005), leaves a root that already holds them as it was (B-006) and writes only through the store's replace (B-007, C-2).
/// </summary>
/// <param name="loader">Reads and validates the manifest as the check does.</param>
/// <param name="parser">Selects the pinned version.</param>
/// <param name="shipped">The embedded files.</param>
/// <param name="store">Reads and replaces the configuration files.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
public sealed class UpgradeWriter(SpecSchemasLoader loader, SpecManifestParser parser, ShippedFiles shipped, SpecManifestStore store)
{
    /// <summary>Rewrites each present file that differs from the pinned version's, and none other.</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="cancellationToken">Cancels the run between files.</param>
    /// <returns>The pin and the rewritten files.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">The root holds no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest cannot be parsed.</exception>
    /// <exception cref="SpechtManifestException">The pin is not shipped, or a write failed.</exception>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    public async Task<UpgradeResult> Upgrade(string root, CancellationToken cancellationToken)
    {
        var structure = loader.LoadManifest(root);
        var pinned = parser.PinnedVersion(structure).Number;
        var frontmatterFromRoot = structure.SchemaSource == SchemaSourceSelection.Disk || structure.SpechtSchema is not null;
        var plan = shipped.At(pinned)
            .Where(static file => file.Relative != SpecManifestStore.RelativePath)
            .Where(file => !frontmatterFromRoot
                           || !file.Relative.StartsWith(SpecManifestStore.FrontmatterSchemaFolder + "/", StringComparison.Ordinal))
            .Where(file => store.ReadConfigurationFile(root, file.Relative) is { } present
                           && !present.AsSpan().SequenceEqual(file.Bytes))
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
