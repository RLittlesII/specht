using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Schema;

/// <summary>Real CLI listing, marking and recovery for 0001-F7 B-041, B-084, B-085 and B-109.</summary>
[Trait("Tier", "Integration")]
public sealed class SchemaListCommandIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Repository_WhenListing_ShouldListEveryShippedVersionAndMarkOnlyItsPin(bool manifestPresent)
    {
        // Given
        using var tree = new SpecTree();
        if (!manifestPresent)
        {
            File.Delete(Path.Combine(tree.Root, SpecTree.ManifestPath));
        }

        var versions = EngineServices.Versions().Versions.Select(static version => version.Number.ToString()).ToArray();

        // When
        var result = AcceptanceTool.Launch(tree.Root, "schema", "list");

        // Then
        result.ExitCode.Should().Be(0, result.Stderr);
        result.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Should().Equal(versions.Select(version => manifestPresent && version == "0.2.0" ? version + " (pinned)" : version));
        result.Stderr.Should().BeEmpty();
    }

    [Fact]
    public void UnsupportedPin_WhenListing_ShouldGiveRecoveryWithoutAList()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw(SpecTree.ManifestPath, "{\"schemaVersion\":\"7.0.0\"}");

        // When
        var result = AcceptanceTool.Launch(tree.Root, "schema", "list");

        // Then
        result.ExitCode.Should().Be(3);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Contain("7.0.0").And.Contain("0.2.0").And.Contain("specht schema pin");
        result.Stderr.Should().NotContain(tree.Root);
    }
}
