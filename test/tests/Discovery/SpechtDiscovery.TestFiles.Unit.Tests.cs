using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;

namespace Specht.Tests.Discovery;

/// <summary>
/// Declared test files are listed by discovery, by root-relative path and without their text, which the tree store reads
/// (C-2, C-3; item 0166).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtDiscoveryTestFilesUnitTests
{
    [Theory]
    [ClassData(typeof(DeclaredTestFiles))]
    public void DeclaredGlobsAndExclusions_WhenTestFilesAreListed_ShouldSelectUniqueFilesInRelativeOrder(
        string[] globs, string[] exclusions, string[] expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/test/obj/c.cs"] = new("excluded"),
            ["repo/test/nested/b.cs"] = new("nested"),
            ["repo/test/a.cs"] = new("first"),
            ["repo/other/d.cs"] = new("other"),
        });

        // When
        var selected = new SpechtDiscovery(files).FindTestFiles("repo", globs, exclusions);

        // Then
        selected.Should().Equal(expected);
    }

    [Fact]
    public void NoDeclaredGlobs_WhenTestFilesAreListed_ShouldNotRequireARootDirectory()
    {
        // Given
        var discovery = new SpechtDiscovery(new MockFileSystem());

        // When
        var files = discovery.FindTestFiles("repo", [], []);

        // Then
        files.Should().BeEmpty();
    }
}
