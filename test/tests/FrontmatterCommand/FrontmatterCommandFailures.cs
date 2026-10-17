namespace Specht.Tests.FrontmatterCommand;

/// <summary>Usage, kind and path failures precede root/manifest loading.</summary>
public sealed class FrontmatterCommandFailures : TheoryData<string[], int, string>
{
    public FrontmatterCommandFailures()
    {
        Add(["--stdin"], 2, "specht frontmatter --kind");
        Add(["--kind", "task"], 2, "specht frontmatter --kind");
        Add(["--kind", "task", "drafts/issue.md", "--stdin"], 2, "specht frontmatter --kind");
        Add(["--kind", "story"], 2, "specht frontmatter --kind");
        Add(["--kind", "story", "drafts/missing.md"], 4, "story");
        Add(["--kind", "task", "drafts/missing.md"], 4, "drafts/missing.md");
        Add(["--kind", "Story", "drafts/issue.md"], 4, "Story");
    }
}
