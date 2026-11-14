namespace Specht.Tests.Facts;

/// <summary>
/// A root-relative directory and the specifications it selects from a document holding
/// <c>src/tool/Features/Facts/.spec/README.md</c>, <c>src/toolbox/.spec/README.md</c>,
/// <c>epics/0001-epic/F1-feature/spec.md</c> and <c>src/tool/.spec/README.md</c> in that order (<c>0132-F2</c> B-019,
/// OQ-4's default): every specification under it by whole path segments, so <c>src/tool</c> does not select
/// <c>src/toolbox</c>, a trailing <c>/</c> names the same directory, a legacy epic folder selects its Feature, and the
/// order is the document's, never sorted.
/// </summary>
public sealed class FactsSelectedDirectories : TheoryData<string, string[]>
{
    public FactsSelectedDirectories()
    {
        Add("src", ["src/tool/Features/Facts/.spec/README.md", "src/toolbox/.spec/README.md", "src/tool/.spec/README.md"]);
        Add("src/tool", ["src/tool/Features/Facts/.spec/README.md", "src/tool/.spec/README.md"]);
        Add("src/tool/", ["src/tool/Features/Facts/.spec/README.md", "src/tool/.spec/README.md"]);
        Add("epics/0001-epic", ["epics/0001-epic/F1-feature/spec.md"]);
    }
}
