using System;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;
using Xunit;

namespace specht.tests;

/// <summary>
/// A test class without exactly one tier trait drops out of both filtered tiers while an
/// unfiltered run still reports it passing (specht-conventions § Testing).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class TestTierGovernanceTests
{
    [Fact]
    public void EveryTestClass_WhenReflected_ShouldDeclareExactlyOneTier()
    {
        // Given
        var testClasses = typeof(TestTierGovernanceTests).Assembly.GetTypes()
            .Where(static type => type.GetMethods().Any(static method => method.IsDefined(typeof(FactAttribute), inherit: true)));

        // When
        var offenders = testClasses
            .Where(static type =>
            {
                var tiers = type.GetCustomAttributes<TraitAttribute>()
                    .Where(static trait => trait.Name == "Tier")
                    .ToArray();
                return tiers.Length != 1 || !Tiers.Contains(tiers[0].Value, StringComparer.Ordinal);
            })
            .Select(static type => type.FullName);

        // Then
        offenders.Should().BeEmpty();
    }

    private static readonly string[] Tiers = ["Unit", "Integration"];
}
