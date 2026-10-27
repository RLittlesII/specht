namespace Specht.Tests.Manifest;

/// <summary>Accepted null declarations retain their identity instead of becoming missing keys.</summary>
public sealed class EffectiveManifestNulls : TheoryData<string>
{
    public EffectiveManifestNulls()
    {
        foreach (var key in new[] { "sections", "tables", "roles", "markers", "identifiers", "frontmatterSchemas", "claim", "rules", "codeAnalysis" })
        {
            Add(key);
        }
    }
}
