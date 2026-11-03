using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Versioning;

/// <summary>
/// The version a pin policy has the check use (<c>0001-F7</c> B-053, B-054, B-147): <see cref="SchemaVersions.InUse"/>
/// returns the held version the policy selects for a held pin, over sets built in memory and given out of order.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SchemaVersionsInUseUnitTests
{
    [Theory]
    [ClassData(typeof(PinPolicySelections))]
    public void AHeldPinUnderAPolicy_WhenTheVersionInUseIsSelected_ShouldBeTheHighestHeldVersionThePolicyAllows(
        string[] held, string pin, PinPolicy policy, string expected)
    {
        // Given
        var versions = new SchemaVersions(held.Select(static number => (SchemaVersion)new SchemaVersionFixture().WithNumber(Version(number))));

        // When
        var inUse = versions.InUse(Version(pin), policy);

        // Then
        inUse.Should().BeSameAs(versions.Find(Version(expected)));
    }

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version) ? version : throw new FormatException(text);
}
