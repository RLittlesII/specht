namespace Specht.Tests.Rules;

/// <summary>A typed relative path and a typed absolute path.</summary>
public sealed class RulesCommandRoots : TheoryData<bool>
{
    public RulesCommandRoots()
    {
        Add(false);
        Add(true);
    }
}
