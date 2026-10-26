using Specht.Manifest;

namespace Specht.Tests.Rules;

/// <summary>Each level the manifest can explicitly select or a page can declare.</summary>
public sealed class RuleCatalogueLevels : TheoryData<SpecRuleLevel>
{
    public RuleCatalogueLevels()
    {
        Add(SpecRuleLevel.Error);
        Add(SpecRuleLevel.Warning);
        Add(SpecRuleLevel.Off);
    }
}
