using System.Collections.ObjectModel;

namespace Specht.Resources;

/// <summary>Captures the engine's raw embedded resources once at composition (ADR-0010 D1).</summary>
/// <implements epic="0001" feature="F4" claim="B-004" />
public sealed class SpechtResources
{
    /// <summary>Reads the default manifest, versioned frontmatter schemas, rule pages and templates.</summary>
    /// <implements epic="0001" feature="F4" claim="B-004" />
    public SpechtResources()
    {
        var assembly = typeof(SpechtResources).Assembly;
        var bytes = assembly.GetManifestResourceNames()
            .Where(static name => name == ManifestName
                || name.StartsWith("manifest/v", StringComparison.Ordinal)
                || name.StartsWith("schema/v", StringComparison.Ordinal)
                || name.StartsWith("rules/v", StringComparison.Ordinal)
                || name.StartsWith("templates/v", StringComparison.Ordinal))
            .ToDictionary(static name => name, name =>
            {
                using var stream = assembly.GetManifestResourceStream(name)!;
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                return copy.ToArray();
            }, StringComparer.Ordinal);
        DefaultManifestBytes = bytes[ManifestName];
        Texts = new ReadOnlyDictionary<string, string>(bytes.ToDictionary(static pair => pair.Key, static pair =>
        {
            using var stream = new MemoryStream(pair.Value);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }, StringComparer.Ordinal));
        DefaultManifest = Texts[ManifestName];
    }

    /// <summary>Gets the raw resource texts by logical name.</summary>
    public IReadOnlyDictionary<string, string> Texts { get; }

    /// <summary>Gets the default manifest's original bytes.</summary>
    public ReadOnlyMemory<byte> DefaultManifestBytes { get; }

    /// <summary>Gets the decoded default manifest text.</summary>
    public string DefaultManifest { get; }

    private const string ManifestName = "specht.default-manifest.json";
}
