using Specht.Manifest;

namespace Specht.Tests.Rules;

/// <summary>Each level the manifest can explicitly select or a page can declare.</summary>
public sealed class RuleCatalogueLevels : TheoryData<SpechtRuleLevel>
{
    public RuleCatalogueLevels()
    {
        Add(SpechtRuleLevel.Error);
        Add(SpechtRuleLevel.Warning);
        Add(SpechtRuleLevel.Off);
    }
}
