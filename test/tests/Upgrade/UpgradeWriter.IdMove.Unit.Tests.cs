using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features;
using Specht.Tool.Features.Upgrade;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The move into <c>ids</c> as <c>specht upgrade</c> runs it under a pin of schema <c>0.4.0</c> (<c>0001-F7</c> B-020,
/// B-155 to B-159; § 7 "The move into ids at schema 0.4.0", "Where it runs"): over every file the tool ships for
/// <c>0.4.0</c>, a manifest whose only fault is the old id settings is rewritten, alone, with them moved into records the
/// check then reads, and a manifest holding settings the move cannot make exactly is refused, naming each such member, before
/// any file is written.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class UpgradeWriterIdMoveUnitTests
{
    [Fact]
    public async Task CurrentFilesAndAZeroFourZeroManifestHoldingOldIdSettings_WhenUpgraded_ShouldRewriteOnlyTheManifestWithThemMovedIntoIds()
    {
        // Given
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        foreach (var file in _shipped.At(new SemanticVersion(0, 4, 0)))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var manifest = JsonNode.Parse(fileSystem.File.ReadAllBytes("repo/.specht/manifest.json"))!.AsObject();
        manifest.Remove("ids").Should().BeTrue();
        manifest["identifiers"]!["claim"] = "^B-[0-9]{4}$";
        manifest["mint"] = new JsonObject { ["claim"] = new JsonObject { ["strategy"] = "nanoid", ["prefix"] = "B-" } };
        fileSystem.AddFile("repo/.specht/manifest.json", new MockFileData(manifest.ToJsonString()) { LastWriteTime = stamp });

        // When
        UpgradeWriter writer = new UpgradeWriterFixture().WithFileSystem(fileSystem);
        var result = await writer.Upgrade("repo", CancellationToken.None);

        // Then
        result.Rewritten.Should().Equal(".specht/manifest.json");
        var written = JsonNode.Parse(fileSystem.File.ReadAllBytes("repo/.specht/manifest.json"))!.AsObject();
        written.Should().NotContainKey("mint");
        written["identifiers"]!.AsObject().Should().NotContainKey("claim");
        var record = written["ids"]!["claim"]!;
        record["plain"]!.GetValue<string>().Should().Be("B-{id}");
        record["id"]!.GetValue<string>().Should().Be("[0-9]{4}");
        JsonNode.DeepEquals(record["mint"], new JsonObject { ["strategy"] = "nanoid" }).Should().BeTrue(record.ToJsonString());
        EngineServices.Manifest(fileSystem, "repo").Ids!["claim"].Id.Should().Be("[0-9]{4}");
        foreach (var file in _shipped.At(new SemanticVersion(0, 4, 0)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.File.GetLastWriteTimeUtc("repo/" + file.Relative).Should().Be(stamp, file.Relative);
        }
    }

    [Fact]
    public async Task AZeroFourZeroManifestHoldingSettingsTheMoveCannotMakeExactly_WhenUpgraded_ShouldThrowNamingEachSuchMemberAndWriteNothing()
    {
        // Given
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        foreach (var file in _shipped.At(new SemanticVersion(0, 3, 1)))
        {
            fileSystem.AddFile("repo/" + file.Relative, new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        fileSystem.AddFile(
            "repo/.specht/manifest.json",
            new MockFileData("""
                {
                  "schemaVersion": "0.4.0",
                  "identifiers": { "claim": "B-[0-9]{3}" },
                  "mint": { "decision": { "strategy": "nanoid", "prefix": "D-" } }
                }
                """) { LastWriteTime = stamp });
        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));

        // When
        UpgradeWriter writer = new UpgradeWriterFixture().WithFileSystem(fileSystem);
        var upgrade = () => writer.Upgrade("repo", CancellationToken.None);

        // Then
        var message = (await upgrade.Should().ThrowExactlyAsync<SpechtManifestException>()).Which.Message;
        message.Should().Contain("identifiers.claim").And.Contain("mint.decision.prefix");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes, path);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    private readonly ShippedFiles _shipped = new ShippedFilesFixture();
}
