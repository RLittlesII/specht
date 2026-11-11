using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The unknown-key gate and the <c>identifiers</c> defaults beneath <c>0001-F19</c> B-014, B-044 and B-045 (C-12 to
/// C-14, C-17; decision 0003): a manifest pinned to schema <c>0.3.1</c> that adds a <c>mint</c> entry in C-12's shape
/// for a type C-13 lists is not rejected, and a pin below it that sets the key still rejects it as one the engine does not
/// know (C-17; item <c>0413</c>, "0.1.0 to 0.3.0 unchanged"); one that declares an <c>adr</c>, <c>lesson</c>, <c>decision</c> or
/// <c>scenario</c> grammar carries it; and <c>0.3.1</c>'s own defaults hold the <c>0.3.0</c> grammars and none for those
/// four. A <c>0.3.1</c> manifest whose only difference from a valid one is a <c>mint</c> entry opting in an <c>epic</c>,
/// <c>feature</c> or <c>task</c> type is not rejected either (B-062; item <c>0421</c>).
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
    [ClassData(typeof(NotMintedTypes))]
    public void AZeroThreeOneManifestOptingInAnEpicFeatureOrTaskType_WhenParsed_ShouldNotBeRejected(string type)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.1",
            ["mint"] = new JsonObject
            {
                [type] = JsonNode.Parse("""{ "strategy": "nanoid", "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 8 } }"""),
            },
        };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().NotThrow();
    }

    [Theory]
    [ClassData(typeof(PinsBeforeMint))]
    public void APinBelowZeroThreeOneSettingMint_WhenParsed_ShouldRejectTheKeyAsOneTheEngineDoesNotKnow(string pin)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = pin, ["mint"] = new JsonObject() };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("does not know the key 'mint'");
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
