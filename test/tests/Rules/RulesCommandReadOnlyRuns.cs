namespace Specht.Tests.Rules;

/// <summary>A successful listing and a rejected manifest both remain read-only.</summary>
public sealed class RulesCommandReadOnlyRuns : TheoryData<bool>
{
    public RulesCommandReadOnlyRuns()
    {
        Add(true);
        Add(false);
    }
}
