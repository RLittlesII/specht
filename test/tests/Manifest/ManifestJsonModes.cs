namespace Specht.Tests.Manifest;

/// <summary>The JSON option's two document selections.</summary>
public sealed class ManifestJsonModes : TheoryData<bool>
{
    public ManifestJsonModes()
    {
        Add(false);
        Add(true);
    }
}
