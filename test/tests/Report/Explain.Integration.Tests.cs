using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>0040: real command dispatch and process streams for 0001-F3 B-019, B-020, B-029 and B-030.</summary>
[Trait("Tier", "Integration")]
public sealed class ExplainIntegrationTests
{
    [Theory]
    [InlineData("0.1.0", "SPEC031")]
    [InlineData("0.2.0", "SPEC034")]
    [InlineData("0.3.0", "SPEC4004")]
    public void AValidPin_WhenExplained_ShouldPrintTheExactPageWithoutCheckingAnInvalidTree(string pin, string id)
    {
        // Given
        using var tree = new SpechtTree();
        File.WriteAllText(Path.Combine(tree.Root, SpechtTree.ManifestPath), $$"""{"schemaVersion":"{{pin}}"}""");
        tree.DeleteFrontmatterSchemas();
        tree.WriteFeature("0001", "F1", featureFile: null);
        var page = EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin).RulePages[id];

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "--root", ".", "--schema-source", "disk", "--explain", id);

        // Then
        ExitCode.Should().Be(0, Stderr);
        Stdout.Should().Be(page);
        Stderr.Should().BeEmpty();
    }

    [Theory]
    [InlineData("0.1.0", "SPEC999")]
    [InlineData("0.1.0", "SPEC034")]
    [InlineData("0.3.0", "SPEC031")]
    public void AnIdOutsideThePin_WhenExplained_ShouldNameItOnStderrAndExitFour(string pin, string id)
    {
        // Given
        using var tree = new SpechtTree();
        File.WriteAllText(Path.Combine(tree.Root, SpechtTree.ManifestPath), $$"""{"schemaVersion":"{{pin}}"}""");

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "--root", ".", "--explain", id);

        // Then
        ExitCode.Should().Be(4, Stderr);
        Stdout.Should().BeEmpty();
        Stderr.Should().Contain(id).And.NotContain(tree.Root);
    }

    [Theory]
    [InlineData(null, 2)]
    [InlineData("{", 3)]
    [InlineData("null", 3)]
    [InlineData("{\"unknown\":true}", 3)]
    [InlineData("{\"schemaVersion\":\"7.0.0\"}", 3)]
    public void ARejectedInput_WhenExplained_ShouldWriteOnlyStderrAndPreserveTheCheckExit(string? manifest, int exit)
    {
        // Given
        using var tree = new SpechtTree();
        var path = Path.Combine(tree.Root, SpechtTree.ManifestPath);
        File.Delete(path);
        if (manifest is not null)
        {
            File.WriteAllText(path, manifest);
        }

        // When
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, "--root", ".", "--explain", "SPEC4004");

        // Then
        ExitCode.Should().Be(exit, Stderr);
        Stdout.Should().BeEmpty();
        Stderr.Should().NotBeEmpty().And.NotContain(tree.Root);
    }
}
