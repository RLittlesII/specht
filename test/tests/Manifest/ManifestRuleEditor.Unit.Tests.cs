using System.Text;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>Proves the byte-preserving edit and insertion cases of 0001-F11.</summary>
[Trait("Tier", "Unit")]
public sealed class ManifestRuleEditorUnitTests
{
    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16")]
    [InlineData("utf-16BE")]
    [InlineData("utf-32")]
    [InlineData("utf-32BE")]
    public void ExistingLevel_WhenChanged_ShouldPreserveEncodingAndEveryOtherByte(string name)
    {
        // Given
        var encoding = Encoding.GetEncoding(name);
        const string text = "{\r\n \"$comment\":\"😀\", \"rules\": {\"SPEC010\" : \"error\",\"SPEC001\":\"off\"},"
            + "\"$nested\":{\"rules\":{\"SPEC010\":\"error\"}}}\r\n";
        byte[] bytes = [.. encoding.GetPreamble(), .. encoding.GetBytes(text)];

        // When
        var changed = new ManifestRuleEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions())).Change(bytes, "SPEC010", "warning");

        // Then
        var expected = text.Replace("\"SPEC010\" : \"error\"", "\"SPEC010\" : \"warning\"", StringComparison.Ordinal);
        changed.Should().Equal([.. encoding.GetPreamble(), .. encoding.GetBytes(expected)]);
        bytes.Should().Equal([.. encoding.GetPreamble(), .. encoding.GetBytes(text)]);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"schemaVersion\":\"0.2.0\",\"$comment\":\"😀\"}")]
    [InlineData("{\"rules\":{}}")]
    [InlineData("{\"rules\":{\"SPEC001\":\"off\"}}")]
    public void MissingEntry_WhenChanged_ShouldKeepOtherValuesAndLeaveAnAcceptedManifest(string text)
    {
        // Given
        var before = JsonNode.Parse(text)!.AsObject();

        // When
        var changed = new ManifestRuleEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()))
            .Change(Encoding.UTF8.GetBytes(text), "SPEC010", "warning");

        // Then
        var after = JsonNode.Parse(changed)!.AsObject();
        after["rules"]!["SPEC010"]!.GetValue<string>().Should().Be("warning");
        EngineServices.Parser().PinnedVersion(EngineServices.Parser().Parse(Encoding.UTF8.GetString(changed)))
            .RuleIds.Should().Contain("SPEC010");
        after["rules"]!.AsObject().Remove("SPEC010");
        if (!before.ContainsKey("rules"))
        {
            after.Remove("rules");
        }
        JsonNode.DeepEquals(before, after).Should().BeTrue();
    }

    [Fact]
    public void NullRulesAcceptedByTheParser_WhenSet_ShouldReplaceNullWithOneEntry()
    {
        // Given
        const string text = """{"rules":null,"$comment":"kept"}""";
        EngineServices.Parser().Parse(text);

        // When
        var changed = new ManifestRuleEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()))
            .Change(Encoding.UTF8.GetBytes(text), "SPEC010", "off");

        // Then
        Encoding.UTF8.GetString(changed).Should().Be("""{"rules":{"SPEC010":"off"},"$comment":"kept"}""");
        EngineServices.Parser().Parse(Encoding.UTF8.GetString(changed)).Rules["SPEC010"].Should().Be(SpecRuleLevel.Off);
    }

    [Fact]
    public void EscapedNamesAndSameLevel_WhenSet_ShouldLeaveEveryByteUnchanged()
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes("{\"\\u0072ules\":{\"SPEC010\":\"warning\"}}\r\n");

        // When
        var changed = new ManifestRuleEditor(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions())).Change(bytes, "SPEC010", "warning");

        // Then
        changed.Should().Equal(bytes);
    }
}
