using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The unknown-key gate and the <c>identifiers</c> defaults beneath <c>0001-F19</c> B-014, B-044 and B-045 (C-12 to
/// C-14, C-17; decision 0003): a manifest pinned to schema <c>0.3.1</c> that adds a <c>mint</c> entry in C-12's shape
/// for a type C-13 lists is not rejected; one that declares an <c>adr</c>, <c>lesson</c>, <c>decision</c> or
/// <c>scenario</c> grammar carries it; and <c>0.3.1</c>'s own defaults hold the <c>0.3.0</c> grammars and none for those
/// four.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserMintUnitTests
{
    [Theory]
    [ClassData(typeof(MintedTypes))]
    public void AZeroThreeOneManifestOptingATypeIn_WhenParsed_ShouldNotBeRejected(string type, string grammar, string entry)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.1",
            ["identifiers"] = new JsonObject { [type] = grammar },
            ["mint"] = new JsonObject { [type] = JsonNode.Parse(entry) },
        };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().NotThrow();
    }

    [Theory]
    [ClassData(typeof(NewTypeGrammars))]
    public void AZeroThreeOneManifestDeclaringAGrammarForANewType_WhenParsed_ShouldCarryThatGrammar(string type, string grammar)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.1",
            ["identifiers"] = new JsonObject { [type] = grammar },
        };

        // When
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        structure.Identifiers.Should().ContainKey(type).WhoseValue.Should().Be(grammar);
    }

    [Fact]
    public void TheZeroThreeOneDefaults_WhenParsed_ShouldHoldTheZeroThreeZeroGrammarsAndNoneForTheFourNewTypes()
    {
        // Given
        var parser = EngineServices.Parser();

        // When
        var patch = parser.Parse("""{ "schemaVersion": "0.3.1" }""").Identifiers;
        var published = parser.Parse("""{ "schemaVersion": "0.3.0" }""").Identifiers;

        // Then
        patch.Should().BeEquivalentTo(published);
        patch.Should().NotContainKeys("adr", "lesson", "decision", "scenario");
    }
}
