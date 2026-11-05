using AwesomeAssertions;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>Holds how the host reads the version a tool states and orders two versions (0132-F3 B-027, B-028).</summary>
[Trait("Tier", "Unit")]
public sealed class ToolVersionUnitTests
{
    [Theory]
    [ClassData(typeof(StatedVersions))]
    public void AVersionThatATooStates_WhenParsed_ShouldYieldItsNumbersAndReadPastTheCommit(string text, int major, int minor, int patch)
    {
        // Given
        // When
        var parsed = ToolVersion.TryParse(text, out var version);

        // Then
        parsed.Should().BeTrue();
        version.Should().Be(new ToolVersion(major, minor, patch));
    }

    [Theory]
    [ClassData(typeof(UnstatedVersions))]
    public void OutputThatStatesNoVersion_WhenParsed_ShouldNotBeAVersion(string text)
    {
        // Given
        // When
        var parsed = ToolVersion.TryParse(text, out _);

        // Then
        parsed.Should().BeFalse();
    }

    [Theory]
    [ClassData(typeof(VersionOrders))]
    public void TwoVersions_WhenCompared_ShouldOrderTheNumbersAsNumbers(string smaller, string larger)
    {
        // Given
        ToolVersion.TryParse(smaller, out var below).Should().BeTrue();
        ToolVersion.TryParse(larger, out var above).Should().BeTrue();

        // When
        var up = below.CompareTo(above);
        var down = above.CompareTo(below);

        // Then
        up.Should().BeNegative();
        down.Should().BePositive();
        below.CompareTo(below).Should().Be(0);
    }
}
