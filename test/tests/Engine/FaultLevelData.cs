namespace Specht.Tests.Engine;

/// <summary>
/// The manifest levels that select a failing rule: its failure is stamped error under either (0001-F1 B-015). A rule set off
/// is not selected, so it never fails.
/// </summary>
public sealed class FaultLevelData : TheoryData<string>
{
    public FaultLevelData()
    {
        Add("error");
        Add("warning");
    }
}
