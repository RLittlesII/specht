using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>Init exposes exactly the pinned vocabulary at its own defaults (0001-F4 B-024 to B-026).</summary>
[Trait("Tier", "Unit")]
public sealed class InitRuleDefaultsUnitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ANewManifest_WhenInitialized_ShouldNameExactlyThePinnedRulesAtTheirOwnDefaultLevels(bool upstream)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.Directory.CreateDirectory("repo");
        InitWriter writer = new InitWriterFixture().WithFileSystem(fileSystem);

        // When
        await writer.Write(
            "repo", schemas: false, templates: false, CancellationToken.None,
            upstream: upstream ? "https://schemas.example.test/frontmatter/" : null,
            upstreamVersion: upstream ? "synthetic-version" : null,
            upstreamHashes: upstream ? ["feature=sha256:" + new string('a', 64), "task=sha256:" + new string('b', 64)] : null);

        // Then
        var manifest = JsonNode.Parse(fileSystem.File.ReadAllText("repo/.specht/manifest.json"))!.AsObject();
        var version = EngineServices.Versions().Versions.Single(version => version.Number.ToString() == manifest["schemaVersion"]!.GetValue<string>());
        var rules = manifest["rules"] as JsonObject;
        rules.Should().NotBeNull("init exposes a default level for every pinned rule");
        rules!.Select(static pair => pair.Key).Should().BeEquivalentTo(version.RuleIds);
        foreach (var id in version.RuleIds)
        {
            var page = RulePage.Read(version.Number, id, version.RulePages[id]);
            var severity = page.Value("Default severity");
            severity.Should().NotBeNullOrEmpty($"{id} documents its default level");
            rules[id]!.GetValue<string>().Should().Be(severity!.ToLowerInvariant(), id);
        }
    }
}
