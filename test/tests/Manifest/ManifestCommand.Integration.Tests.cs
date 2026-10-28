using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Exercises the shipped manifest command's output and failure boundary (0001-F9).</summary>
[Trait("Tier", "Integration")]
public sealed class ManifestCommandIntegrationTests
{
    [Fact]
    public void AValidRoot_WhenPrinting_ShouldEmitOnlyTheEffectiveDocumentInDefaultOrder()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw(SpecTree.ManifestPath, "{\"roles\":{\"claims\":\"3. Acceptance Criteria\"},\"schemaVersion\":\"0.1.0\"}");
        tree.WriteRaw("broken/.spec/README.md", "not a specification");
        var expected = EngineServices.EffectiveManifest(tree.Root);
        var defaults = JsonNode.Parse(EngineServices.Versions().Versions[0].Manifest)!.AsObject();

        // When
        var result = AcceptanceTool.Launch(tree.Root, "manifest");

        // Then
        result.ExitCode.Should().Be(0, result.Stderr);
        result.Stderr.Should().BeEmpty();
        var document = JsonNode.Parse(result.Stdout)!.AsObject();
        JsonNode.DeepEquals(document, expected).Should().BeTrue();
        document.Select(static entry => entry.Key).Where(defaults.ContainsKey).Should()
            .Equal(defaults.Select(static entry => entry.Key).Where(document.ContainsKey));
        result.Stdout.Should().Contain("\n  \"");
        document["rules"]!.AsObject().Select(static entry => entry.Key).Should().BeInAscendingOrder(StringComparer.Ordinal);
    }

    [Theory]
    [ClassData(typeof(ManifestCommandFailures))]
    public void InvalidInput_WhenPrinting_ShouldMatchTheChecksFailure(string? manifest, string root, int exitCode)
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
        var check = AcceptanceTool.Launch(tree.Root, "--root", root);

        // When
        var result = AcceptanceTool.Launch(tree.Root, "manifest", "--root", root);

        // Then
        check.ExitCode.Should().Be(exitCode, check.Stderr);
        result.ExitCode.Should().Be(exitCode, result.Stderr);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Be(check.Stderr).And.NotContain(tree.Root).And.NotContain("\\");
    }

    [Fact]
    public void RepeatedRuns_WhenPrinting_ShouldPreserveFilesAndOutput()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("sentinel.bin", "unchanged");
        var before = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path), static path => File.ReadAllBytes(path));

        // When
        var first = AcceptanceTool.Launch(tree.Root, "manifest", "--root", tree.Root);
        var second = AcceptanceTool.Launch(tree.Root, "manifest", "--root", tree.Root);

        // Then
        first.ExitCode.Should().Be(0, first.Stderr);
        second.Should().Be(first);
        first.Stdout.Should().NotContain(tree.Root);
        var after = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path), static path => File.ReadAllBytes(path));
        after.Should().BeEquivalentTo(before);
    }
}
