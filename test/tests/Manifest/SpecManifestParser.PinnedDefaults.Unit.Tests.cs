using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Resources;
using Specht.Tool;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>Pins exact-version defaults, merge boundaries and parse isolation (0001-F5 B-048).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestParserPinnedDefaultsUnitTests
{
    public static TheoryData<string, string> Pins => new()
    {
        { "{}", "Older" },
        { """{"schemaVersion":"0.1.0"}""", "Older" },
        { """{"schemaVersion":"0.1.1"}""", "Newer" },
    };

    [Theory]
    [MemberData(nameof(Pins))]
    public void OmittedValues_WhenParsed_ShouldUseTheExactPinsDefaults(string manifest, string label)
    {
        // Given
        var data = new PinnedDefaultsData();
        var parser = new SpecManifestParser(data.Versions);

        // When
        var structure = parser.Parse(manifest);

        // Then
        structure.SchemaVersion.Should().Be(new SemanticVersion(0, 1, label == "Older" ? 0 : 1));
        structure.Sections.Should().Equal($"3. {label} claims", $"9. {label} matrix", $"12. {label} sign-off");
        structure.Roles["matrix"].Should().Be($"9. {label} matrix");
        structure.Markers["missing"].Should().Be($"{label}Missing");
        structure.Identifiers["claim"].Should().Be($"^{label}-[0-9]+$");
        structure.FrontmatterSchemas["feature"].Should().Be($"{label}.schema.json");
        structure.Tables.Should().ContainSingle().Which.Value.Should().Equal($"{label} ID", "Test");
        structure.Discovery.Layouts.Should().ContainSingle().Which.Name.Should().Be(label);
        structure.Discovery.Exclusions.Should().Equal(label);
        structure.Discovery.TaskFiles.Should().Equal(label);
        structure.Discovery.EpicFiles.Should().Equal(label);
        structure.Discovery.CompanionFiles.Should().Equal(label);
        structure.Rules.Should().BeEmpty();
        structure.SpechtSchema.Should().BeNull();
    }

    [Fact]
    public void ExplicitNamesAndCollections_WhenParsed_ShouldWinWithoutFillingWholeDeclarations()
    {
        // Given
        var data = new PinnedDefaultsData();
        var parser = new SpecManifestParser(data.Versions);
        const string manifest = """
            {
              "schemaVersion":"0.1.0",
              "sections":["3. Claims","9. Older matrix","12. Older sign-off"],
              "roles":{"claims":"3. Claims"},
              "markers":{"missing":"TBD"},
              "identifiers":{"claim":"^REQ-[0-9]+$"},
              "frontmatterSchemas":{"feature":"consumer.json"},
              "tables":{"claims":["Consumer ID"]},
              "layouts":[{"name":"consumer","glob":"custom/**/*.md"}],
              "exclusions":[], "taskFiles":["item.md"], "epicFiles":["epic.md"], "companionFiles":["scenario.feature"]
            }
            """;

        // When
        var structure = parser.Parse(manifest);

        // Then
        structure.Sections.Should().Equal("3. Claims", "9. Older matrix", "12. Older sign-off");
        structure.Roles["claims"].Should().Be("3. Claims");
        structure.Roles["matrix"].Should().Be("9. Older matrix");
        structure.Markers["missing"].Should().Be("TBD");
        structure.Markers["draft"].Should().Be("🟡");
        structure.Identifiers["claim"].Should().Be("^REQ-[0-9]+$");
        structure.Identifiers.Should().ContainKey("task");
        structure.FrontmatterSchemas["feature"].Should().Be("consumer.json");
        structure.FrontmatterSchemas["task"].Should().Be("task.schema.json");
        structure.Tables.Should().ContainSingle().Which.Key.Should().Be("claims");
        structure.Tables["claims"].Should().Equal("Consumer ID");
        structure.Discovery.Layouts.Should().ContainSingle().Which.Glob.Should().Be("custom/**/*.md");
        structure.Discovery.Exclusions.Should().BeEmpty();
        structure.Discovery.TaskFiles.Should().Equal("item.md");
        structure.Discovery.EpicFiles.Should().Equal("epic.md");
        structure.Discovery.CompanionFiles.Should().Equal("scenario.feature");
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"rules\":{}}")]
    [InlineData("{\"rules\":{\"SPEC010\":\"off\"}}")]
    public void UnsetConsumerRuleLevels_WhenParsed_ShouldNotInheritThePinnedDefaultsLevels(string manifest)
    {
        // Given
        var data = new PinnedDefaultsData();
        var parser = new SpecManifestParser(data.Versions);

        // When
        var structure = parser.Parse(manifest);

        // Then
        structure.Markers["missing"].Should().Be("OlderMissing");
        structure.Rules.Should().NotContainKey("SPEC060");
        structure.Rules.Count.Should().Be(manifest.Contains("SPEC010", StringComparison.Ordinal) ? 1 : 0);
    }

    [Theory]
    [InlineData("taskFiles")]
    [InlineData("epicFiles")]
    [InlineData("companionFiles")]
    public void AnExplicitEmptyFileList_WhenParsed_ShouldBeRejectedInsteadOfFilled(string key)
    {
        // Given
        var data = new PinnedDefaultsData();
        var parser = new SpecManifestParser(data.Versions);

        // When
        var parse = () => parser.Parse(new JsonObject { [key] = new JsonArray() }.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain($"the list '{key}' is empty");
    }

    [Fact]
    public async Task OneParserServingAlternatingAndConcurrentPins_WhenParsed_ShouldKeepEachRootsDefaultsIsolated()
    {
        // Given
        var data = new PinnedDefaultsData();
        var parser = new SpecManifestParser(data.Versions);
        var inputs = Enumerable.Range(0, 64).Select(index => new JsonObject { ["schemaVersion"] = index % 2 == 0 ? "0.1.0" : "0.1.1" })
            .Select(static manifest => manifest.ToJsonString()).ToArray();

        // When
        var alternating = inputs.Take(3).Select(text => parser.Parse(text).Markers["missing"]).ToArray();
        var concurrent = await Task.WhenAll(inputs.Select(text => Task.Run(() => parser.Parse(text).Markers["missing"])));

        // Then
        alternating.Should().Equal("OlderMissing", "NewerMissing", "OlderMissing");
        concurrent.Should().Equal(Enumerable.Range(0, inputs.Length).Select(static index => index % 2 == 0 ? "OlderMissing" : "NewerMissing"));
        parser.Parse(inputs[0]).Markers["missing"].Should().Be("OlderMissing");
    }

    [Fact]
    public void EveryEmbeddedDefault_WhenRead_ShouldUseTheHostsOriginalVersionedBytes()
    {
        // Given
        var resources = new SpechtResources();
        var versions = new SchemaVersions(resources.Texts);
        var engine = typeof(SpechtResources).Assembly;
        var host = typeof(ExitCodes).Assembly;

        // When
        var names = versions.Versions.Select(static version => $"manifest/v{version.Number}/manifest.json").ToArray();

        // Then
        foreach (var name in names)
        {
            resources.Texts.Should().ContainKey(name);
            using var engineStream = engine.GetManifestResourceStream(name);
            using var hostStream = host.GetManifestResourceStream(name);
            engineStream.Should().NotBeNull();
            hostStream.Should().NotBeNull();
            using var engineBytes = new MemoryStream();
            using var hostBytes = new MemoryStream();
            engineStream!.CopyTo(engineBytes);
            hostStream!.CopyTo(hostBytes);
            engineBytes.ToArray().Should().Equal(hostBytes.ToArray());
        }
    }
}
