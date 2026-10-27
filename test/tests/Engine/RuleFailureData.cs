namespace Specht.Tests.Engine;

/// <summary>Both immediate failures and failures after lazy output must be isolated.</summary>
public sealed class RuleFailureData : TheoryData<int, bool>
{
    public RuleFailureData()
    {
        Add(0, true);
        Add(0, false);
        Add(2, false);
    }
}
