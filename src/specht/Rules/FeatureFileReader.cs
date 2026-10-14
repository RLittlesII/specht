using System.IO.Abstractions;
using Specht.Manifest;
using Specht.Model;

namespace Specht.Rules;

/// <summary>Reads companion text for the pure tag parser until ADR-0010 moves the read to model loading.</summary>
/// <param name="fileSystem">The file system the companion is read from.</param>
/// <param name="parser">The pure claim-tag parser.</param>
public sealed class FeatureFileReader(IFileSystem fileSystem, FeatureTagParser parser)
{
    /// <summary>Reads the file's claim tags using the supplied manifest, or the legacy contract when absent.</summary>
    /// <param name="absolutePath">The companion file to read.</param>
    /// <param name="structure">The effective manifest.</param>
    /// <returns>Claim tags with original spelling and one-based line numbers.</returns>
    public IReadOnlyList<FeatureTag> ReadTags(string absolutePath, SpecStructure? structure = null) =>
        parser.Parse(fileSystem.File.ReadAllText(absolutePath), structure);
}
