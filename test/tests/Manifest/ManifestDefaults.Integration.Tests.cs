using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Exercises root-independent shipped-default printing (0001-F9 B-008 to B-010, B-012).</summary>
[Trait("Tier", "Integration")]
public sealed class ManifestDefaultsIntegrationTests
{
    [Theory]
    [ClassData(typeof(ManifestCommandFailures))]
    public void AnyRoot_WhenPrintingDefaults_ShouldIgnoreTheRootAndPrintTheNewestEmbeddedDocument(string? manifest, string root, int expectedCheckExit)
    {
        // Given
        using var tree = new SpecTree();
        if (manifest is null)
        {
            File.Delete(Path.Combine(tree.Root, SpecTree.ManifestPath));
        }
        else
        {
            tree.WriteRaw(SpecTree.ManifestPath, manifest);
        }
        AcceptanceTool.Launch(tree.Root, "--root", root).ExitCode.Should().Be(expectedCheckExit);
        var expected = JsonNode.Parse(EngineServices.Versions().Versions[^1].Manifest);
        var before = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path), static path => File.ReadAllBytes(path));

        // When
        var result = AcceptanceTool.Launch(tree.Root, "manifest", "--defaults", "--root", root);

        // Then
        result.ExitCode.Should().Be(0, result.Stderr);
        result.Stderr.Should().BeEmpty();
        JsonNode.DeepEquals(JsonNode.Parse(result.Stdout), expected).Should().BeTrue();
        var after = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path), static path => File.ReadAllBytes(path));
        after.Should().BeEquivalentTo(before);
    }
}
