using Specht.Manifest;

namespace Specht.Tests.Rules;

/// <summary>Synthetic page metadata values independent of the shipped catalogue.</summary>
public sealed class RulePageMetadata : TheoryData<string, SpecRuleLevel>
{
    public RulePageMetadata()
    {
        Add("Fixture structure", SpecRuleLevel.Error);
        Add("Fixture relationship", SpecRuleLevel.Warning);
        Add("Fixture lifecycle", SpecRuleLevel.Off);
    }
}
