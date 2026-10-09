using AwesomeAssertions;

namespace specht.tests;

/// <summary>
/// The root-relative path mapping every engine path passes through (<c>0001-F3</c> B-021): relative to the root, with
/// <c>/</c> separators.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecDiscoveryUnitTests
{
    /// <summary>Gets paths under a root, as segments, and the root-relative path each maps to.</summary>
    public static TheoryData<string[], string> Paths =>
        new()
        {
            { ["README.md"], "README.md" },
            { ["src", "area", ".spec", "README.md"], "src/area/.spec/README.md" },
            { ["epics", "0001-epic", "F1-feature", "spec.md"], "epics/0001-epic/F1-feature/spec.md" },
        };

    [Theory]
    [MemberData(nameof(Paths))]
    public void APathUnderTheRoot_WhenMadeRelative_ShouldBeRelativeToTheRootWithForwardSlashes(string[] segments, string expected)
    {
        // Given
        const string root = "tree";
        var path = Path.Combine([root, .. segments]);

        // When
        var relative = SpecDiscovery.Relative(root, path);

        // Then
        relative.Should().Be(expected);
    }
}
