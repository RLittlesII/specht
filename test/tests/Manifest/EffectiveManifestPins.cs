namespace Specht.Tests.Manifest;

/// <summary>Exact pins and the synthetic defaults that distinguish them.</summary>
public sealed class EffectiveManifestPins : TheoryData<string, string>
{
    public EffectiveManifestPins()
    {
        Add("0.1.0", "Older");
        Add("0.2.0", "Newer");
    }
}
