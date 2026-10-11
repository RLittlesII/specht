using System.IO.Abstractions;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Model;

/// <summary>The manifest stage: loads the manifest and the pinned version's schemas through an injected file system (ADR-0005 (a)).</summary>
/// <param name="fileSystem">The file system the manifest is read through.</param>
public sealed class SpecSchemasLoader(IFileSystem fileSystem)
{
    /// <summary>Loads the manifest under <paramref name="root"/> and the schemas of the version it pins from the embedded version set.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The schemas and the structure the manifest declares.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse.</exception>
    /// <exception cref="SpechtManifestException">The manifest is rejected.</exception>
    public SpecSchemas Load(string root) => Load(root, SchemaVersions.Embedded);

    /// <summary>
    /// Loads the manifest under <paramref name="root"/> and the schemas of the version it pins from
    /// <paramref name="versions"/> (<c>0001-F7</c> B-001, C-4).
    /// </summary>
    /// <param name="root">The repository root.</param>
    /// <param name="versions">The version set the pin selects from.</param>
    /// <returns>The schemas and the structure the manifest declares.</returns>
    /// <exception cref="SpechtRootNotFoundException"><paramref name="root"/> is not a directory.</exception>
    /// <exception cref="SpechtManifestNotFoundException">There is no manifest.</exception>
    /// <exception cref="SpechtManifestUnreadableException">The manifest does not parse.</exception>
    /// <exception cref="SpechtManifestException">The manifest is rejected, or pins a version <paramref name="versions"/> does not hold.</exception>
    public SpecSchemas Load(string root, SchemaVersions versions) => SpecSchemas.Load(fileSystem, root, versions);
}
