using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Schema file names are validated in memory before any schema read (0001-F5 B-047).</summary>
[Trait("Tier", "Unit")]
public sealed class SpecManifestParserSchemaFileNamesUnitTests
{
    [Theory]
    [InlineData("feature", "/feature.json")]
    [InlineData("task", "../item.json")]
    [InlineData("epic", "nested/epic.json")]
    [InlineData("feature", "nested\\feature.json")]
    [InlineData("task", "item..json")]
    [InlineData("epic", "..")]
    [InlineData("feature", "C:\\feature.json")]
    [InlineData("task", "\\item.json")]
    [InlineData("epic", "\\\\host\\epic.json")]
    public void APathInASchemaFileName_WhenParsed_ShouldRejectItNamingTheKindAndValue(string kind, string value)
    {
        // Given
        var parser = EngineServices.Parser();
        var manifest = new JsonObject { ["frontmatterSchemas"] = new JsonObject { [kind] = value } };

        // When
        var parse = () => parser.Parse(manifest.ToJsonString());

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Be(
            $".specht/manifest.json: the frontmatterSchemas kind '{kind}' names '{value}', which is not a plain file name.");
    }
}
