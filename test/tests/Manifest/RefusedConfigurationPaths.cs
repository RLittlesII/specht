namespace Specht.Tests.Manifest;

/// <summary>Root-relative paths outside the manifest, the frontmatter schema folder and the templates folder (<c>0001-F7</c> B-007, C-2).</summary>
public sealed class RefusedConfigurationPaths : TheoryData<string>
{
    public RefusedConfigurationPaths()
    {
        Add("README.md");
        Add(".spec/README.md");
        Add(".specht/schema/notes.md");
        Add(".specht/schemas/frontmatter/feature.schema.json");
        Add(".specht/templatesx/feature.md");
        Add(".specht/templates/../manifest.bak");
        Add("../outside.md");
        Add(".specht\\templates\\feature.md");
        Add(".specht/templates/..\\manifest.bak");
    }
}
