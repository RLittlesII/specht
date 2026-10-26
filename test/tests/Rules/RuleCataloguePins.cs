namespace Specht.Tests.Rules;

/// <summary>Two exact pins with distinct synthetic vocabularies.</summary>
public sealed class RuleCataloguePins : TheoryData<string, string>
{
    public RuleCataloguePins()
    {
        Add("0.1.0", "SPEC001");
        Add("0.2.0", "SPEC010");
    }
}
