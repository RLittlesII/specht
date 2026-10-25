using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Specht.Manifest;
using Specht.Versioning;

namespace Specht.Tool.Features;

/// <summary>
/// The manifest, frontmatter schemas and templates the tool embeds, mapped to the root-relative paths <c>init</c> writes and
/// <c>upgrade</c> compares (<c>0001-F4</c> B-029, <c>0001-F7</c> B-005).
/// </summary>
/// <param name="shippingCopy">The embedded files by logical name, as <c>InitWriter.ShippingCopy</c> reads them.</param>
/// <implements epic="0001" feature="F7" claim="B-005" />
public sealed partial class ShippedFiles(IReadOnlyDictionary<string, byte[]> shippingCopy)
{
    /// <summary>Matches a logical resource name of a shipped file.</summary>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    [GeneratedRegex(
        "^(?:manifest/v(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)/manifest\\.json|"
        + "(?<folder>schema|templates)/v(?<version>[0-9]+\\.[0-9]+\\.[0-9]+)/(?<path>.+))$")]
    public static partial Regex Pattern();

    /// <summary>Gets the highest shipped version.</summary>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    public SemanticVersion Newest() => Parsed().Max(static entry => entry.Version);

    /// <summary>Lists one version's files in ordinal path order.</summary>
    /// <param name="version">The shipped version.</param>
    /// <returns>Each file's root-relative path and bytes.</returns>
    /// <implements epic="0001" feature="F7" claim="B-005" />
    public IReadOnlyList<ShippedFile> At(SemanticVersion version) =>
        Parsed()
            .Where(entry => entry.Version == version)
            .Select(static entry => new ShippedFile(entry.Relative, entry.Bytes))
            .OrderBy(static file => file.Relative, StringComparer.Ordinal)
            .ToList();

    /// <implements epic="0001" feature="F7" claim="B-005" />
    private IEnumerable<(SemanticVersion Version, string Relative, byte[] Bytes)> Parsed() =>
        shippingCopy
            .Select(static entry => (Name: Pattern().Match(entry.Key), Bytes: entry.Value))
            .Where(static entry => entry.Name.Success)
            .Select(static entry => (
                Version: SemanticVersion.TryParse(entry.Name.Groups["version"].Value, out var version)
                    ? version
                    : throw new InvalidOperationException($"{entry.Name.Value} is embedded with an invalid schema version."),
                Relative: entry.Name.Groups["folder"].Success
                    ? $"{SpecManifestStore.ConfigurationFolder}/{entry.Name.Groups["folder"].Value}/{entry.Name.Groups["path"].Value}"
                    : SpecManifestStore.RelativePath,
                entry.Bytes));
}
