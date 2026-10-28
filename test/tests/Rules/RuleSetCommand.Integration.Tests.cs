using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>Checks shipped CLI composition, branch option placement and deterministic rule writes.</summary>
[Trait("Tier", "Integration")]
public sealed class RuleSetCommandIntegrationTests
{
    [Theory]
    [InlineData("0.1.0", "error", true)]
    [InlineData("0.2.0", "warning", false)]
    [InlineData("0.2.0", "off", true)]
    public void AcceptedManifest_WhenSettingThroughTheHost_ShouldWriteOnlyTheNamedOverride(string pin, string level, bool rootBeforeCommand)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw(SpecTree.ManifestPath, $$$"""{"schemaVersion":"{{{pin}}}","rules":{"SPEC001":"off"}}""");
        string[] arguments = rootBeforeCommand
            ? ["rules", "--root", ".", "set", "SPEC010", level]
            : ["rules", "set", "SPEC010", level, "--root", "."];
        var before = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .Where(path => path != Path.Combine(tree.Root, SpecTree.ManifestPath))
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path).Replace('\\', '/'), static path => File.ReadAllBytes(path));

        // When
        var result = AcceptanceTool.Launch(tree.Root, arguments);
        var bytes = File.ReadAllBytes(Path.Combine(tree.Root, SpecTree.ManifestPath));
        var repeat = AcceptanceTool.Launch(tree.Root, arguments);

        // Then
        result.ExitCode.Should().Be(0, result.Stderr);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().BeEmpty();
        repeat.Should().Be(result);
        File.ReadAllBytes(Path.Combine(tree.Root, SpecTree.ManifestPath)).Should().Equal(bytes);
        var manifest = JsonNode.Parse(bytes)!;
        manifest["schemaVersion"]!.GetValue<string>().Should().Be(pin);
        manifest["rules"]!["SPEC010"]!.GetValue<string>().Should().Be(level);
        manifest["rules"]!["SPEC001"]!.GetValue<string>().Should().Be("off");
        var after = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .Where(path => path != Path.Combine(tree.Root, SpecTree.ManifestPath))
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path).Replace('\\', '/'), static path => File.ReadAllBytes(path));
        after.Should().BeEquivalentTo(before);
    }
}
