using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>Pins the text command's process boundary and input-failure mapping (0001-F10 B-001, B-011 to B-019).</summary>
[Trait("Tier", "Integration")]
public sealed class RulesCommandIntegrationTests
{
    [Theory]
    [ClassData(typeof(RulesCommandPins))]
    public void APinnedManifest_WhenListing_ShouldPrintOnlyThatCatalogueRegardlessOfTreeContent(string pin, bool explicitRoot)
    {
        // Given
        using var tree = new SpecTree();
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(tree.Root, SpecTree.ManifestPath)))!.AsObject();
        manifest["schemaVersion"] = pin;
        tree.WriteRaw(SpecTree.ManifestPath, manifest.ToJsonString());
        tree.WriteRaw("broken/.spec/README.md", "not a specification");
        var version = EngineServices.Versions().Versions.Single(version => version.Number.ToString() == pin);
        string[] arguments = explicitRoot ? ["rules", "--root", "."] : ["rules"];

        // When
        var result = AcceptanceTool.Launch(tree.Root, arguments);

        // Then
        result.ExitCode.Should().Be(0, result.Stderr);
        result.Stderr.Should().BeEmpty();
        var lines = result.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(static line => line.TrimEnd('\r').Split('\t')).ToArray();
        lines.Should().HaveCount(version.RuleIds.Count);
        lines.Should().OnlyContain(static fields => fields.Length == 4);
        lines.Select(static fields => fields[0]).Should().Equal(version.RuleIds.Order(StringComparer.Ordinal));
        lines.Should().ContainSingle(static fields => fields.SequenceEqual(new[] { "SPEC010", "Structure", "error", "error" }));
        foreach (var line in lines)
        {
            line[2].Should().BeOneOf("error", "warning", "off");
            line[3].Should().BeOneOf("error", "warning", "off");
        }
    }

    [Theory]
    [ClassData(typeof(RulesCommandRejectedManifests))]
    public void ARejectedManifest_WhenListing_ShouldMatchTheChecksErrorAndLeaveStdoutEmpty(string manifest, string namedFault)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw(SpecTree.ManifestPath, manifest);
        var check = AcceptanceTool.Launch(tree.Root);

        // When
        var result = AcceptanceTool.Launch(tree.Root, "rules", "--root", ".");

        // Then
        check.ExitCode.Should().Be(3, check.Stderr);
        check.Stderr.Should().Contain(namedFault);
        result.ExitCode.Should().Be(3);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Be(check.Stderr).And.NotContain(tree.Root).And.NotContain("\\");
    }

    [Theory]
    [ClassData(typeof(RulesCommandRoots))]
    public void AMissingRoot_WhenListing_ShouldEchoOnlyTheTypedRootOnStderr(bool absolute)
    {
        // Given
        using var tree = new SpecTree();
        var typedRoot = absolute ? Path.Combine(tree.Root, "absent") : "absent";

        // When
        var result = AcceptanceTool.Launch(tree.Root, "rules", "--root", typedRoot);

        // Then
        result.ExitCode.Should().Be(2);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Be($"specht: '{typedRoot}' is not a directory.{Environment.NewLine}");
    }

    [Fact]
    public void ARootWithoutAManifest_WhenListing_ShouldNameOnlyTheRelativeManifestPath()
    {
        // Given
        using var tree = new SpecTree();
        File.Delete(Path.Combine(tree.Root, SpecTree.ManifestPath));
        var check = AcceptanceTool.Launch(tree.Root);

        // When
        var result = AcceptanceTool.Launch(tree.Root, "rules", "--root", tree.Root);

        // Then
        result.ExitCode.Should().Be(2);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Be(check.Stderr).And.Contain(SpecTree.ManifestPath).And.NotContain(tree.Root).And.NotContain("\\");
    }

    [Theory]
    [ClassData(typeof(RulesCommandReadOnlyRuns))]
    public void ARun_WhenListing_ShouldLeaveEveryOwnedFileUnchanged(bool valid)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw("sentinel.bin", "unchanged");
        if (!valid)
        {
            tree.WriteRaw(SpecTree.ManifestPath, "{");
        }
        var before = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path).Replace('\\', '/'), static path => File.ReadAllBytes(path));

        // When
        var first = AcceptanceTool.Launch(tree.Root, "rules");
        var second = AcceptanceTool.Launch(tree.Root, "rules");

        // Then
        first.ExitCode.Should().Be(valid ? 0 : 3, first.Stderr);
        second.ExitCode.Should().Be(first.ExitCode);
        second.Stdout.Should().Be(first.Stdout);
        second.Stderr.Should().Be(first.Stderr);
        var after = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path).Replace('\\', '/'), static path => File.ReadAllBytes(path));
        after.Should().BeEquivalentTo(before);
    }
}
