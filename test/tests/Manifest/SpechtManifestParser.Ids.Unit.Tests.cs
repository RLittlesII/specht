using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The <c>ids</c> key's gate, shape and record checks (<c>0001-F5</c> B-020, B-081 to B-085, B-092 to B-094; decision
/// 0024; item <c>0436</c>). A manifest pinned to 0.4.0 is parsed against <see cref="IdsVersion"/>'s catalog, whose
/// default records are decision 0024's; a pin below 0.4.0 against the shipped catalog. Each row is the manifest's members
/// beside <c>schemaVersion</c>, as JSON, and a rejection is read for the members it names.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtManifestParserIdsUnitTests
{
    [Theory]
    [ClassData(typeof(AcceptedIdRecords))]
    public void AZeroFourZeroManifestWhoseIdsRecordsAreWellFormed_WhenParsed_ShouldNotBeRejected(string members)
    {
        // Given
        var manifest = ZeroFourZero(members);

        // When
        var parse = () => new SpechtManifestParser(IdsVersion.Catalog()).Parse(manifest);

        // Then
        parse.Should().NotThrow();
    }

    [Theory]
    [ClassData(typeof(PinsBeforeIds))]
    public void APinBelowZeroFourZeroHoldingIds_WhenParsed_ShouldRejectTheKeyAsOneTheEngineDoesNotKnow(string pin)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = pin,
            ["ids"] = new JsonObject { ["claim"] = new JsonObject { ["plain"] = "B-{id}", ["id"] = "[0-9]{3}" } },
        };

        // When
        var parse = () => EngineServices.Parser().Parse(manifest.ToJsonString());

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("does not know the key 'ids'");
    }

    [Theory]
    [ClassData(typeof(MalformedIdsKeys))]
    public void AZeroFourZeroManifestWhoseIdsKeyIsMalformed_WhenParsed_ShouldBeRejectedNamingTheMember(string members, string member)
    {
        // Given
        var manifest = ZeroFourZero(members);

        // When
        var parse = () => new SpechtManifestParser(IdsVersion.Catalog()).Parse(manifest);

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain(member);
    }

    [Theory]
    [ClassData(typeof(MalformedIdTexts))]
    public void AZeroFourZeroRecordWhosePlainOrFormIsNotOneClosingId_WhenParsed_ShouldBeRejectedNamingTheTypeAndTheText(
        string members,
        string type,
        string text)
    {
        // Given
        var manifest = ZeroFourZero(members);

        // When
        var parse = () => new SpechtManifestParser(IdsVersion.Catalog()).Parse(manifest);

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain($"ids.{type}").And.Contain(text);
    }

    [Theory]
    [ClassData(typeof(UnusableIdPatterns))]
    public void AZeroFourZeroRecordWhoseIdIsNotAUsablePattern_WhenParsed_ShouldBeRejectedNamingTheTypeAndTheValue(string members, string value)
    {
        // Given
        var manifest = ZeroFourZero(members);

        // When
        var parse = () => new SpechtManifestParser(IdsVersion.Catalog()).Parse(manifest);

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("ids.claim").And.Contain(value);
    }

    [Theory]
    [ClassData(typeof(OldIdSettings))]
    public void AZeroFourZeroManifestHoldingAnOldIdSetting_WhenParsed_ShouldBeRejectedNamingTheIdsRecordOfEachTypeItHolds(
        string members,
        string[] records)
    {
        // Given
        var manifest = ZeroFourZero(members);

        // When
        var parse = () => new SpechtManifestParser(IdsVersion.Catalog()).Parse(manifest);

        // Then
        var message = parse.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        foreach (var record in records)
        {
            message.Should().Contain(record);
        }
    }

    [Theory]
    [ClassData(typeof(SharedClaimAndScenarioHeads))]
    public void AZeroFourZeroManifestWhoseClaimAndScenarioRecordsShareAHead_WhenParsed_ShouldBeRejectedNamingBothRecords(string members)
    {
        // Given
        var manifest = ZeroFourZero(members);

        // When
        var parse = () => new SpechtManifestParser(IdsVersion.Catalog()).Parse(manifest);

        // Then
        parse.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Contain("ids.claim").And.Contain("ids.scenario");
    }

    private static string ZeroFourZero(string members)
    {
        var manifest = JsonNode.Parse(members)!.AsObject();
        manifest.Insert(0, "schemaVersion", IdsVersion.Number.ToString());
        return manifest.ToJsonString();
    }
}
