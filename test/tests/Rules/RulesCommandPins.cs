namespace Specht.Tests.Rules;

/// <summary>Both shipped pins and the explicit/default root forms.</summary>
public sealed class RulesCommandPins : TheoryData<string, bool>
{
    public RulesCommandPins()
    {
        Add("0.1.0", false);
        Add("0.2.0", true);
    }
}
