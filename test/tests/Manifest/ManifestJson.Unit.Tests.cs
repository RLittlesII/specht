using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool.Features.Manifest;
using Spectre.Console.Testing;

namespace Specht.Tests.Manifest;

/// <summary>Pins the JSON option's document selection through the direct command.</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestJsonUnitTests
{
    [Theory]
    [ClassData(typeof(ManifestJsonModes))]
    public async Task EitherSelection_WhenExecutedAsJson_ShouldKeepTheSelectedDocumentsValues(bool defaults)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData> { ["repo/.specht/manifest.json"] = new("{}") });
        var schemas = EngineServices.Schemas(files);
        var reader = new EffectiveManifestReader(schemas, new RuleEntriesReader(schemas, EngineServices.Parser()), EngineServices.Versions());
        var console = new TestConsole();
        using var errors = new StringWriter(CultureInfo.InvariantCulture);
        var command = new ManifestCommand(console, reader, errors);
        var settings = new ManifestCommand.Settings { Root = files.Path.GetFullPath("repo"), Defaults = defaults, Json = true };
        var expected = defaults ? reader.ReadDefaultText() : reader.ReadText("repo");

        // When
        var exit = await command.ExecuteAsync(null!, settings, TestContext.Current.CancellationToken);

        // Then
        exit.Should().Be(0);
        errors.ToString().Should().BeEmpty();
        JsonNode.DeepEquals(JsonNode.Parse(console.Output), JsonNode.Parse(expected)).Should().BeTrue();
    }
}
