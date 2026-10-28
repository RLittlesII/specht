using Specht.Manifest;

namespace Specht.Tests.Engine;

/// <summary>The existing input failures retain their command-owned exit mapping.</summary>
public sealed class TypedInputFailureData : TheoryData<Exception>
{
    public TypedInputFailureData()
    {
        Add(new SpechtRootNotFoundException());
        Add(new SpechtManifestNotFoundException(false));
        Add(new SpechtManifestUnreadableException(new FormatException()));
        Add(new SpechtManifestException(".specht/schema/frontmatter/feature.schema.json: invalid schema."));
    }
}
