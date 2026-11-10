namespace Specht.Tests.Model;

/// <summary>
/// Why a row asks as it does, and two specifications, the earlier and the later in one ordinal order of root-relative
/// path, each with an item beside it, both items <c>0001-01</c>: one specification below the other's directory, and one
/// in each layout with the co-located path sorting before <c>epics/</c> (<c>0001-F6</c> B-008, decision 0011; item 0009).
/// </summary>
public sealed class ReusedItemIds : TheoryData<string, string, string, string, string>
{
    public ReusedItemIds()
    {
        Add(
            "an item below the other specification's directory sorts after the item beside it",
            "epics/0001-e/F1-f/spec.md",
            "epics/0001-e/F1-f/0001-01-a.md",
            "epics/0001-e/F1-f/G1-g/spec.md",
            "epics/0001-e/F1-f/G1-g/0001-01-b.md");
        Add(
            "a co-located specification whose path sorts before epics/ is read before the epics one",
            "docs/x/.spec/README.md",
            "docs/x/.spec/0001-01-a.md",
            "epics/0001-e/F1-f/spec.md",
            "epics/0001-e/F1-f/0001-01-b.md");
    }
}
