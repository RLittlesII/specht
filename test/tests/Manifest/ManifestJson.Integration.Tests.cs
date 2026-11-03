using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Pins exactly-one-document output for both JSON modes (0001-F9 B-011, B-020).</summary>
[Trait("Tier", "Integration")]
public sealed class ManifestJsonIntegrationTests
{
    [Theory]
    [ClassData(typeof(ManifestJsonModes))]
    public void EitherSelection_WhenPrintingJson_ShouldEmitOneDocumentEquivalentToTheTextForm(bool defaults)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, "{}");
        string[] textArguments = defaults ? ["manifest", "--defaults"] : ["manifest"];
        var (Stdout, Stderr, ExitCode) = AcceptanceTool.Launch(tree.Root, textArguments);

        // When
        var help = AcceptanceTool.Launch(tree.Root, "manifest", "--help");
        var json = AcceptanceTool.Launch(tree.Root, [.. textArguments, "--json"]);

        // Then
        help.Stdout.Should().Contain("--json");
        ExitCode.Should().Be(0, Stderr);
        json.ExitCode.Should().Be(0, json.Stderr);
        json.Stderr.Should().BeEmpty();
        JsonNode.DeepEquals(JsonNode.Parse(json.Stdout), JsonNode.Parse(Stdout)).Should().BeTrue();
    }
}
