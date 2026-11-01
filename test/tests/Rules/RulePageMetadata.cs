using Specht.Manifest;

namespace Specht.Tests.Rules;

/// <summary>Synthetic page metadata values independent of the shipped catalogue.</summary>
public sealed class RulePageMetadata : TheoryData<string, SpechtRuleLevel>
{
    public RulePageMetadata()
    {
        Add("Fixture structure", SpechtRuleLevel.Error);
        Add("Fixture relationship", SpechtRuleLevel.Warning);
        Add("Fixture lifecycle", SpechtRuleLevel.Off);
    }
}
