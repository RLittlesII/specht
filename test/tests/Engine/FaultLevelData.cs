namespace Specht.Tests.Engine;

/// <summary>A family still selected through its secondary id reports a primary-id fault even when that id is off.</summary>
public sealed class FaultLevelData : TheoryData<string>
{
    public FaultLevelData()
    {
        Add("error");
        Add("warning");
        Add("off");
    }
}
