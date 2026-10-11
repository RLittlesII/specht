using AwesomeAssertions;
using Specht.Rules;

namespace Specht.Tests.Rules;

/// <summary>
/// The hand-written rule list (ADR-0001 stage C; <c>0001-F1</c> B-004, C-9): every rule the engine assembly declares is
/// on it once, and it carries them in the order the runner evaluated them in when it found them by reflection.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecRulesUnitTests
{
    [Fact]
    public void EveryConcreteRuleTheEngineAssemblyDeclares_WhenTheRuleListIsRead_ShouldBeOnItExactlyOnce()
    {
        // Given
        var declared = typeof(ISpecRule).Assembly
            .GetTypes()
            .Where(static type => typeof(ISpecRule).IsAssignableFrom(type) && type is { IsAbstract: false, IsInterface: false })
            .Select(static type => type.FullName)
            .ToList();

        // When
        var listed = SpecRules.All.Select(static rule => rule.GetType().FullName).ToList();

        // Then
        listed.Should().BeEquivalentTo(declared);
    }

    [Fact]
    public void TheRuleList_WhenRead_ShouldCarryItsRulesInOrdinalOrderOfTheirIds()
    {
        // Given
        var rules = SpecRules.All;

        // When
        var ids = rules.Select(static rule => rule.Id).ToList();

        // Then
        ids.Should().BeInAscendingOrder(StringComparer.Ordinal);
    }
}
