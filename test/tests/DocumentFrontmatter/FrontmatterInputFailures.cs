using Specht.Manifest;

namespace Specht.Tests.DocumentFrontmatter;

/// <summary>The check's existing typed failures, retained by standalone frontmatter evaluation.</summary>
public sealed class FrontmatterInputFailures : TheoryData<bool, string?, Type>
{
    public FrontmatterInputFailures()
    {
        Add(false, null, typeof(SpechtRootNotFoundException));
        Add(true, null, typeof(SpechtManifestNotFoundException));
        Add(true, "{", typeof(SpechtManifestUnreadableException));
        Add(true, """{ "unknown": true }""", typeof(SpechtManifestException));
        Add(true, """{ "schemaVersion": "99.0.0" }""", typeof(SpechtManifestException));
    }
}
