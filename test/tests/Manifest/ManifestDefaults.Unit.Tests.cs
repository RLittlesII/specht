using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Manifest;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Tests.Manifest;

/// <summary>Pins newest-default selection without loading or expanding a consumer root.</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestDefaultsUnitTests
{
    [Fact]
    public async Task NoReadableRoot_WhenPrintingDefaults_ShouldUseTheNewestEmbeddedValuesWithoutProjection()
    {
        // Given
        var data = new PinnedDefaultsData();
        var embedded = JsonNode.Parse(data.Versions.Versions[^1].Manifest)!.AsObject();
        embedded["rules"] = new JsonObject { ["SPEC020"] = "off", ["SPEC010"] = "warning" };
        var versions = new SchemaVersions([data.Versions.Versions[0], data.Versions.Versions[^1] with { Manifest = embedded.ToJsonString() }]);
        var files = new MockFileSystem();
        var schemas = EngineServices.Schemas(files, versions);
        var reader = new EffectiveManifestReader(schemas, new RuleEntriesReader(schemas, EngineServices.Parser()), versions);
        var expected = JsonNode.Parse(versions.Versions[^1].Manifest);
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new ManifestCommand(console, reader, errors);

        // When
        var exit = await command.ExecuteAsync(null!, new ManifestCommand.Settings { Defaults = true, Root = "\0" }, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0);
        errors.ToString().Should().BeEmpty();
        JsonNode.DeepEquals(JsonNode.Parse(console.Output), expected).Should().BeTrue();
        files.AllFiles.Should().BeEmpty();
        JsonNode.Parse(console.Output)!["rules"]!.AsObject().Select(static entry => entry.Key).Should().Equal("SPEC010", "SPEC020");
        JsonNode.DeepEquals(JsonNode.Parse(reader.ReadDefaultText()), expected).Should().BeTrue();
    }
}
