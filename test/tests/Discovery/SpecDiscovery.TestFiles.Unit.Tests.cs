using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;

namespace Specht.Tests.Discovery;

/// <summary>Declared test-file selection stays within the existing discovery store (C-2, C-3).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecDiscoveryTestFilesUnitTests
{
    [Theory]
    [ClassData(typeof(DeclaredTestFiles))]
    public void DeclaredGlobsAndExclusions_WhenTestFilesAreRead_ShouldSelectUniqueFilesInRelativeOrder(
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
        var selected = new SpecDiscovery(files).ReadTestFiles("repo", globs, exclusions);

        // Then
        selected.Select(static file => file.RelativePath).Should().Equal(expected);
        selected.Select(static file => file.Text).Should().OnlyContain(static text => text.Length > 0);
    }

    [Fact]
    public void NoDeclaredGlobs_WhenTestFilesAreRead_ShouldNotRequireARootDirectory()
    {
        // Given
        var discovery = new SpecDiscovery(new MockFileSystem());

        // When
        var files = discovery.ReadTestFiles("repo", [], []);

        // Then
        files.Should().BeEmpty();
    }
}
