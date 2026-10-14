using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>Token-level preservation beneath 0001-F7 B-045 and missing-key insertion beneath B-047.</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestPinEditorUnitTests
{
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("utf-16BE")]
    [InlineData("utf-32")]
    [InlineData("utf-32BE")]
    public void SupportedEncoding_WhenPinning_ShouldPreserveItsBomAndEveryOtherByte(string name)
    {
        // Given
        var encoding = Encoding.GetEncoding(name);
        const string text = "{\r\n  \"$comment\": \"😀 before pin\",\r\n  \"schemaVersion\" : \"0.1.0\",\r\n  \"$nested\": {\"schemaVersion\":\"0.1.0\"}\r\n}";
        byte[] bytes = [.. encoding.GetPreamble(), .. encoding.GetBytes(text)];
        var editor = new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions());

        // When
        editor.Validate(bytes);
        var changed = editor.Change(bytes, new SemanticVersion(0, 2, 0));

        // Then
        var expected = text.Replace("\"schemaVersion\" : \"0.1.0\"", "\"schemaVersion\" : \"0.2.0\"", StringComparison.Ordinal);
        changed.Should().Equal([.. encoding.GetPreamble(), .. encoding.GetBytes(expected)]);
        bytes.Should().Equal([.. encoding.GetPreamble(), .. encoding.GetBytes(text)]);
    }

    [Theory]
    [InlineData("{\"schemaVersion\" : \"0.1.0\", \"$metadata\":{\"schemaVersion\":\"0.1.0\"}}",
        "{\"schemaVersion\" : \"0.2.0\", \"$metadata\":{\"schemaVersion\":\"0.1.0\"}}")]
    [InlineData("{\"\\u0073chemaVersion\":\"0.1.0\",\"$comment\":\"😀\"}\r\n",
        "{\"\\u0073chemaVersion\":\"0.2.0\",\"$comment\":\"😀\"}\r\n")]
    public void ExistingRootPin_WhenChanged_ShouldPreserveEveryOtherByte(string original, string expected)
    {
        // Given
        var editor = new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions());
        byte[] bytes = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes(original)];

        // When
        var result = editor.Change(bytes, new SemanticVersion(0, 2, 0));

        // Then
        result.Should().Equal([0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes(expected)]);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{ \"$comment\": \"😀\" }\r\n")]
    public void MissingPin_WhenChanged_ShouldAddItAndPreserveTheOtherValues(string original)
    {
        // Given
        var editor = new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions());
        var before = JsonNode.Parse(original)!.AsObject();

        // When
        var result = JsonNode.Parse(editor.Change(Encoding.UTF8.GetBytes(original), new SemanticVersion(0, 2, 0)))!.AsObject();

        // Then
        result["schemaVersion"]!.GetValue<string>().Should().Be("0.2.0");
        result.Remove("schemaVersion");
        JsonNode.DeepEquals(before, result).Should().BeTrue();
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("{}")]
    [InlineData("[1,2]")]
    public void NonStringCurrentPin_WhenValidatedAndChanged_ShouldRepairOnlyItsWholeValue(string value)
    {
        // Given
        var editor = new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions());
        var original = "{\"schemaVersion\":" + value + ",\"rules\":{\"SPEC031\":\"warning\"}}";
        var bytes = Encoding.UTF8.GetBytes(original);

        // When
        editor.Validate(bytes);
        var result = editor.Change(bytes, new SemanticVersion(0, 2, 0));

        // Then
        Encoding.UTF8.GetString(result).Should().Be("{\"schemaVersion\":\"0.2.0\",\"rules\":{\"SPEC031\":\"warning\"}}");
        bytes.Should().Equal(Encoding.UTF8.GetBytes(original));
    }
}
