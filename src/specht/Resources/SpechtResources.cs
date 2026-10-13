namespace Specht.Resources;

/// <summary>Reads the engine's embedded resource text at the I/O boundary (ADR-0010).</summary>
public sealed class SpechtResources
{
    /// <summary>Reads the default manifest embedded from the repository's live copy.</summary>
    public SpechtResources()
    {
        using var stream = typeof(SpechtResources).Assembly.GetManifestResourceStream("specht.default-manifest.json")!;
        using var reader = new StreamReader(stream);
        DefaultManifest = reader.ReadToEnd();
    }

    /// <summary>Gets the embedded default manifest text.</summary>
    public string DefaultManifest { get; }
}
