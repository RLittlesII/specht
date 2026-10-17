using Specht.Model;

namespace Specht.Tests.DocumentFrontmatter;

/// <summary>The manifest's exact kind names and their frontmatter rule ids.</summary>
public sealed class FrontmatterKinds : TheoryData<string, FrontmatterKind, string>
{
    public FrontmatterKinds()
    {
        Add("feature", FrontmatterKind.Feature, "SPEC002");
        Add("task", FrontmatterKind.Task, "SPEC003");
        Add("epic", FrontmatterKind.Epic, "SPEC004");
    }
}
