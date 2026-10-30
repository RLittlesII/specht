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
/// B-005), or each present schema and each present template a recorded upstream source hashes to the content its hash names
/// (B-026, B-033, B-130, B-134, B-136), adds the keys the pinned version defines and the manifest lacks (B-016), leaves a
/// root that already holds them as it was (B-006) and writes only through the store's replace once every read, comparison
/// and fetch has succeeded (B-007, B-027, B-035, B-132, B-133, C-2). Under the on-disk source it plans no frontmatter
/// schema (B-029, B-031) and, with no upstream source recorded, lists each one the pinned version ships that the root holds
/// as skipped (B-030, B-143).
/// </summary>
/// <param name="loader">Reads and validates the manifest as the check does.</param>
/// <param name="parser">Selects the pinned version.</param>
/// <param name="shipped">The embedded files.</param>
/// <param name="store">Reads and replaces the configuration files.</param>
/// <param name="keys">Adds the pinned version's missing keys to the manifest's bytes.</param>
/// <param name="fetcher">Fetches and verifies a recorded upstream schema or template.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
/// <implements epic="0001" feature="F7" claim="B-016" />
/// <implements epic="0001" feature="F7" claim="B-026" />
/// <implements epic="0001" feature="F7" claim="B-130" />
/// <implements epic="0001" feature="F7" claim="B-030" />
public sealed class UpgradeWriter(
    SpecSchemasLoader loader,
    SpecManifestParser parser,
    ShippedFiles shipped,
    SpecManifestStore store,
    ManifestKeyEditor keys,
    UpstreamSchemaFetcher fetcher)
{
    /// <summary>
    /// Rewrites each present file that differs from the pinned version's, each present schema or template a recorded source
    /// hashes that differs from its hash, and the manifest when it lacks a key that version defines, and none other; under
    /// the on-disk source with no recorded source, lists each present shipped frontmatter schema as skipped without comparing
    /// or writing it.
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="cancellationToken">Cancels the run between files.</param>
    /// <returns>The pin, the rewritten files and the skipped files.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">The root holds no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest cannot be parsed.</exception>
    /// <exception cref="SpechtManifestException">The pin is not shipped, a schema name leaves its folder, or a write failed.</exception>
    /// <exception cref="SpechtUpstreamSchemaException">
    /// A recorded source is unreachable, refuses, or serves a schema or template that fails its hash.
    /// </exception>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    /// <implements epic="0001" feature="F7" claim="B-016" />
    /// <implements epic="0001" feature="F7" claim="B-059" />
    /// <implements epic="0001" feature="F7" claim="B-026" />
    /// <implements epic="0001" feature="F7" claim="B-027" />
    /// <implements epic="0001" feature="F7" claim="B-033" />
    /// <implements epic="0001" feature="F7" claim="B-034" />
    /// <implements epic="0001" feature="F7" claim="B-035" />
    /// <implements epic="0001" feature="F7" claim="B-067" />
    /// <implements epic="0001" feature="F7" claim="B-082" />
    /// <implements epic="0001" feature="F7" claim="B-139" />
    /// <implements epic="0001" feature="F7" claim="B-140" />
    /// <implements epic="0001" feature="F7" claim="B-130" />
    /// <implements epic="0001" feature="F7" claim="B-132" />
    /// <implements epic="0001" feature="F7" claim="B-133" />
    /// <implements epic="0001" feature="F7" claim="B-134" />
    /// <implements epic="0001" feature="F7" claim="B-136" />
    /// <implements epic="0001" feature="F7" claim="B-137" />
    /// <implements epic="0001" feature="F7" claim="B-141" />
    /// <implements epic="0001" feature="F7" claim="B-021" />
    /// <implements epic="0001" feature="F7" claim="B-029" />
    /// <implements epic="0001" feature="F7" claim="B-030" />
    /// <implements epic="0001" feature="F7" claim="B-031" />
    /// <implements epic="0001" feature="F7" claim="B-143" />
    public async Task<UpgradeResult> Upgrade(string root, CancellationToken cancellationToken)
    {
        var structure = loader.LoadManifest(root);
        var pinned = parser.PinnedVersion(structure).Number;
        var frontmatterFromRoot = structure.SchemaSource == SchemaSourceSelection.Disk || structure.SpechtSchema is not null;
        var hashedTemplates = (structure.SpechtSchema?.Templates ?? [])
            .Select(static file => (File: file, Relative: SpecManifestStore.TemplateFolder + "/" + file.FileName))
            .ToList();
        var skipped = structure.SchemaSource == SchemaSourceSelection.Disk && structure.SpechtSchema is null
            ? shipped.At(pinned)
                .Where(static file => file.Relative.StartsWith(SpecManifestStore.FrontmatterSchemaFolder + "/", StringComparison.Ordinal))
                .Where(file => store.ReadConfigurationFile(root, file.Relative) is not null)
                .Select(static file => new UpgradeSkip(file.Relative, "the on-disk source is selected"))
                .OrderBy(static skip => skip.Relative, StringComparer.Ordinal)
                .ToList()
            : [];
        var plan = shipped.At(pinned)
            .Where(file => !frontmatterFromRoot
                           || !file.Relative.StartsWith(SpecManifestStore.FrontmatterSchemaFolder + "/", StringComparison.Ordinal))
            .Where(file => !hashedTemplates.Exists(hashed => hashed.Relative == file.Relative))
            .Select(file => (File: file, Present: store.ReadConfigurationFile(root, file.Relative)))
            .Where(static entry => entry.Present is not null)
            .Select(entry => (entry.Present, Planned: new ShippedFile(
                entry.File.Relative,
                entry.File.Relative == SpecManifestStore.RelativePath ? keys.AddMissing(entry.Present!, entry.File.Bytes) : entry.File.Bytes)))
            .Where(static entry => !entry.Present.AsSpan().SequenceEqual(entry.Planned.Bytes))
            .Select(static entry => entry.Planned)
            .ToList();
        var differing = (structure.SpechtSchema?.Files ?? [])
            .Select(file => (File: file, Relative: SpecManifestStore.FrontmatterSchemaFolder + "/" + file.FileName))
            .Select(entry => (entry.File, entry.Relative, Present: store.ReadConfigurationFile(root, entry.Relative)))
            .Where(static entry => entry.Present is not null && !entry.File.Matches(entry.Present))
            .ToList();
        var differingTemplates = hashedTemplates
            .Where(entry => !structure.ConsumerTemplates.Contains(entry.File.FileName, StringComparer.Ordinal))
            .Select(entry => (entry.File, entry.Relative, Present: store.ReadConfigurationFile(root, entry.Relative)))
            .Where(static entry => entry.Present is not null && !entry.File.Matches(entry.Present))
            .ToList();
        foreach (var entry in differing)
        {
            plan.Add(new ShippedFile(entry.Relative, await fetcher.Fetch(entry.File, cancellationToken)));
        }

        foreach (var entry in differingTemplates)
        {
            plan.Add(new ShippedFile(entry.Relative, await fetcher.Fetch(entry.File, cancellationToken)));
        }

        plan.Sort(static (left, right) => string.CompareOrdinal(left.Relative, right.Relative));

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

        return new UpgradeResult(pinned, plan.Select(static file => file.Relative).ToList(), skipped);
    }
}
