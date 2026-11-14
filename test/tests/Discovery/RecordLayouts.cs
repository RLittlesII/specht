namespace Specht.Tests.Discovery;

/// <summary>
/// A specification's root-relative path and the folder that holds it, in each layout the records are found beside
/// (<c>0132-F2</c> B-011): the co-located <c>.spec/</c> and a legacy Feature's own folder.
/// </summary>
public sealed class RecordLayouts : TheoryData<string, string>
{
    public RecordLayouts()
    {
        Add("src/area/.spec/README.md", "src/area/.spec");
        Add("epics/0001-epic/F1-feature/spec.md", "epics/0001-epic/F1-feature");
    }
}
