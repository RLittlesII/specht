using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Proves a nonempty report survives writing the projected manifest into a synthetic copy (0001-F9 B-007).</summary>
[Trait("Tier", "Integration")]
public sealed class EffectiveManifestReaderIntegrationTests
{
    [Fact]
    public void AViolatingRootWithoutEpicGrammar_WhenCopiedWithItsEffectiveManifest_ShouldKeepTheCompleteReport()
    {
        // Given
        using var original = new SpecTree();
        using var copy = new SpecTree();
        original.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria",
            """
            ## 3. Acceptance Criteria

            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | B-001 | Invented requirement. | synthetic | Active |
            | B-001 | Duplicate. | synthetic | Active |
            """));
        var path = Path.Combine(original.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["schemaVersion"] = "0.2.0";
        manifest["schemaSource"] = "disk";
        manifest["roles"]!.AsObject().Remove("claims");
        manifest["identifiers"]!.AsObject().Remove("epic");
        File.WriteAllText(path, manifest.ToJsonString());
        File.Delete(Path.Combine(original.Root, SpecTree.EpicSchemaPath));
        File.Delete(Path.Combine(copy.Root, SpecTree.EpicSchemaPath));
        foreach (var file in Directory.EnumerateFiles(original.Root, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(copy.Root, Path.GetRelativePath(original.Root, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, overwrite: true);
        }

        // When
        var before = original.Run();
        var effective = EngineServices.EffectiveManifest(original.Root);
        File.WriteAllText(Path.Combine(copy.Root, SpecTree.ManifestPath), effective.ToJsonString());
        var after = copy.Run();

        // Then
        before.Violations.Should().NotBeEmpty();
        SpecReportDocument.From(after).ToJson().Should().Be(SpecReportDocument.From(before).ToJson());
        after.SpecificationCount.Should().Be(before.SpecificationCount);
        effective["identifiers"]!.AsObject().Should().NotContainKey("epic");
        File.ReadAllText(path).Should().Be(manifest.ToJsonString());
    }
}
