using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The <c>claim</c> members beneath <c>0001-F16</c> B-002, B-027, B-028, B-029 and B-031 (item <c>0389</c>): from schema
/// <c>0.4.0</c> the parser reads the claim source and the claim file sets, a source left out or <c>markdown</c> reads as
/// Markdown with no set, a set's identity follows the epic grammar, an invalid set is rejected naming
/// <c>claim.files</c>, and a source outside the two is rejected naming <c>claim.source</c>; below <c>0.4.0</c> the
/// members go unread. Each test parses with the shipped catalog, whose <c>0.4.0</c> manifest carries the
/// <c>claim</c> defaults <c>0001-F16</c> § 7 names.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserClaimMembersUnitTests
{
    [Theory]
    [ClassData(typeof(MarkdownClaimSources))]
    public void AManifestWithoutAGherkinClaimSource_WhenParsedUnderZeroFourZero_ShouldReadMarkdownClaimsAndNoClaimFileSet(string members)
    {
        // Given
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest["schemaVersion"] = "0.4.0";

        // When
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        structure.ClaimInput.Should().NotBeNull();
        structure.ClaimInput!.Source.Should().Be(SpechtClaimSource.Markdown);
        structure.ClaimInput.Files.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ClaimFileSetIdentities))]
    public void AClaimFileSet_WhenParsedUnderEitherEpicGrammar_ShouldTakeTheIdentityThatGrammarGives(
        string members, string? epic, string feature)
    {
        // Given
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest["schemaVersion"] = "0.4.0";

        // When
        var structure = EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        structure.ClaimInput.Should().NotBeNull();
        structure.ClaimInput!.Source.Should().Be(SpechtClaimSource.Gherkin);
        structure.ClaimInput.Files.Should().ContainSingle()
            .Which.Should().Match<SpechtClaimFileSet>(set => set.Epic == epic && set.Feature == feature);
    }

    [Theory]
    [ClassData(typeof(InvalidClaimFileSets))]
    public void AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles(string members)
    {
        // Given
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest["schemaVersion"] = "0.4.0";

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("claim.files");
    }

    [Theory]
    [ClassData(typeof(UndefinedClaimSources))]
    public void AClaimSourceOutsideTheTwo_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimSource(string source)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.4.0",
            ["claim"] = new JsonObject { ["source"] = JsonNode.Parse(source) },
        };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("claim.source");
    }

    [Theory]
    [ClassData(typeof(ClaimMembersBelowZeroFourZero))]
    public void AClaimMemberUnderAPinBelowZeroFourZero_WhenParsed_ShouldReadTheManifestAsWithoutIt(string pin, string members)
    {
        // Given
        var parser = EngineServices.Parser();
        var without = new JsonObject { ["schemaVersion"] = pin };
        var with = new JsonObject { ["schemaVersion"] = pin, ["claim"] = JsonNode.Parse(members) };

        // When
        var read = parser.Parse(with.ToJsonString());

        // Then
        read.Should().BeEquivalentTo(parser.Parse(without.ToJsonString()), static options => options.ComparingRecordsByMembers());
    }
}
