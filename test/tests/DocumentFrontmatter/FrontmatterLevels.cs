namespace Specht.Tests.DocumentFrontmatter;

/// <summary>Manifest overrides and their expected standalone report counts.</summary>
public sealed class FrontmatterLevels : TheoryData<string, int, int>
{
    public FrontmatterLevels()
    {
        Add("{}", 2, 0);
        Add("""{ "rules": { "SPEC003": "error" } }""", 2, 0);
        Add("""{ "rules": { "SPEC003": "warning" } }""", 0, 2);
        Add("""{ "rules": { "SPEC003": "off" } }""", 0, 0);
    }
}
