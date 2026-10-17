namespace Specht.Tests.FrontmatterCommand;

/// <summary>Each kind keeps its rule id and the caller's selected input label.</summary>
public sealed class FrontmatterCommandKinds : TheoryData<string, string>
{
    public FrontmatterCommandKinds()
    {
        Add("feature", "SPEC002");
        Add("task", "SPEC003");
        Add("epic", "SPEC004");
    }
}
