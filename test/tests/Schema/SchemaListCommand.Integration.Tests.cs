using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Tests.Schema;

/// <summary>
/// Real CLI listing, marking and recovery for 0001-F7 B-041, B-084, B-085 and B-109, and, through the schema slice's
/// production registrations over a synthetic set, the in-use mark under a floating pin policy (B-148, B-149).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SchemaListCommandIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Repository_WhenListing_ShouldListEveryShippedVersionAndMarkOnlyItsPin(bool manifestPresent)
    {
        // Given
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, "{\"schemaVersion\":\"0.2.0\"}");
        if (!manifestPresent)
        {
            File.Delete(Path.Combine(tree.Root, SpechtTree.ManifestPath));
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
        using var tree = new SpechtTree();
        tree.WriteRaw(SpechtTree.ManifestPath, "{\"schemaVersion\":\"7.0.0\"}");

        // When
        var result = AcceptanceTool.Launch(tree.Root, "schema", "list");

        // Then
        result.ExitCode.Should().Be(3);
        result.Stdout.Should().BeEmpty();
        result.Stderr.Should().Contain("7.0.0").And.Contain("0.2.0").And.Contain("specht schema pin");
        result.Stderr.Should().NotContain(tree.Root);
    }

    [Theory]
    [ClassData(typeof(PinPolicyMarks))]
    public void AManifestPinningAVersionWithLatestPatch_WhenListed_ShouldMarkThePinAndTheVersionInUseOnlyWhereTheyDiffer(
        string[] held, string[] lines)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var files = new MockFileSystem();
        files.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData("""{ "schemaVersion": "0.3.0", "pinPolicy": "latest-patch" }"""));
        var embedded = EngineServices.Versions().Find(new SemanticVersion(0, 3, 0))!;
        var versions = new SchemaVersions(held.Select(number => embedded with
        {
            Number = SemanticVersion.TryParse(number, out var version) ? version : throw new FormatException(number),
        }));
        var console = new TestConsole();
        var app = EngineServices.CommandApp(files, console, versions);

        // When
        var result = app.Run("schema", "list", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Should().Equal(lines);
    }
}
