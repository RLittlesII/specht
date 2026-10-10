using AwesomeAssertions;

namespace Specht.Tests;

/// <summary>
/// A schema version as <c>major.minor.patch</c> (<c>0001-F7</c> B-001, B-004, B-039, B-055; decision 0005): the text that
/// is one, the text that is not, the order of two versions, and the text a version writes.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SemanticVersionUnitTests
{
    /// <summary>Gets texts of three numbers, and the major, minor and patch each one names.</summary>
    public static TheoryData<string, string, int, int, int> Versions =>
        new()
        {
            { "the first schema version", "0.1.0", 0, 1, 0 },
            { "a patch", "0.1.1", 0, 1, 1 },
            { "a number of two digits", "0.10.0", 0, 10, 0 },
            { "a major", "7.0.0", 7, 0, 0 },
        };

    /// <summary>Gets texts that are not three numbers (B-039).</summary>
    public static TheoryData<string, string> NotVersions =>
        new()
        {
            { "one number", "1" },
            { "two numbers", "0.1" },
            { "four numbers", "0.1.0.0" },
            { "a prefix", "v0.1.0" },
            { "a part that is not a number", "0.1.x" },
        };

    /// <summary>Gets a version and one that follows it.</summary>
    public static TheoryData<string, SemanticVersion, SemanticVersion> Ordered =>
        new()
        {
            { "a later patch follows", new SemanticVersion(0, 1, 0), new SemanticVersion(0, 1, 1) },
            { "a later minor follows a later patch", new SemanticVersion(0, 1, 1), new SemanticVersion(0, 2, 0) },
            { "minors are compared as numbers, not as text", new SemanticVersion(0, 9, 0), new SemanticVersion(0, 10, 0) },
            { "a later major follows a later minor and patch", new SemanticVersion(0, 10, 9), new SemanticVersion(1, 0, 0) },
        };

    [Theory]
    [MemberData(nameof(Versions))]
    public void TextOfThreeNumbers_WhenParsed_ShouldBeThatMajorMinorAndPatch(string because, string text, int major, int minor, int patch)
    {
        // Given
        var expected = new SemanticVersion(major, minor, patch);

        // When
        var parsed = SemanticVersion.TryParse(text, out var version);

        // Then
        parsed.Should().BeTrue(because);
        version.Should().Be(expected, because);
    }

    [Theory]
    [MemberData(nameof(NotVersions))]
    public void TextThatIsNotThreeNumbers_WhenParsed_ShouldNotBeAVersion(string because, string text)
    {
        // Given
        var written = text;

        // When
        var parsed = SemanticVersion.TryParse(written, out _);

        // Then
        parsed.Should().BeFalse(because);
    }

    [Theory]
    [MemberData(nameof(Ordered))]
    public void TwoVersions_WhenCompared_ShouldOrderByMajorThenMinorThenPatch(string because, SemanticVersion earlier, SemanticVersion later)
    {
        // Given
        var pair = (earlier, later);

        // When
        var forwards = pair.earlier.CompareTo(pair.later);
        var backwards = pair.later.CompareTo(pair.earlier);

        // Then
        forwards.Should().BeNegative(because);
        backwards.Should().BePositive(because);
    }

    [Fact]
    public void AVersion_WhenWritten_ShouldBeItsMajorMinorAndPatchSeparatedByDots()
    {
        // Given
        var version = new SemanticVersion(0, 10, 1);

        // When
        var text = version.ToString();

        // Then
        text.Should().Be("0.10.1");
    }
}
