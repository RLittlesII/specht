using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Manifest;

/// <summary>
/// The move beneath <c>specht upgrade</c> at schema <c>0.4.0</c> (<c>0001-F7</c> B-155 to B-161, C-19; § 7 "The move into
/// ids at schema 0.4.0"): a customised grammar becomes its record's plain text and id, a grammar equal to a shipped default
/// below <c>0.4.0</c> is dropped so the type reads the shipped <c>0.4.0</c> record, a <c>mint</c> entry becomes its record's
/// <c>mint</c> less its prefix, or as it is when it is not an object, no old id setting is left, every other member is kept as
/// written in the file's own encoding, a setting the move cannot make exactly is refused naming each such member as the
/// consumer wrote it, a <c>mint</c> entry for a name outside the seven types among them, and a manifest with nothing to move,
/// or pinned below <c>0.4.0</c>, is returned as it is.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ManifestIdMoverUnitTests
{
    [Theory]
    [ClassData(typeof(MovedIdGrammars))]
    public void ACustomisedGrammarOfTheSplitShape_WhenMoved_ShouldBecomeARecordOfItsHeadThenIdPlaceholderAndItsPattern(
        string type, string grammar, string plain, string id)
    {
        // Given
        var manifest = new JsonObject { ["schemaVersion"] = "0.4.0", ["identifiers"] = new JsonObject { [type] = grammar } };
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moved = mover.Move(manifest.ToJsonString());

        // Then
        var record = JsonNode.Parse(moved)!["ids"]![type]!;
        record["plain"]!.GetValue<string>().Should().Be(plain);
        record["id"]!.GetValue<string>().Should().Be(id);
    }

    [Theory]
    [ClassData(typeof(DefaultIdGrammars))]
    public void AGrammarEqualToAShippedDefaultBelowZeroFourZero_WhenMoved_ShouldWriteNoRecordSoTheTypeReadsTheShippedRecord(string type, string grammar)
    {
        // Given
        var text = $$$"""{"schemaVersion":"0.4.0","identifiers":{"{{{type}}}":{{{grammar}}}}}""";
        var shipped = EngineServices.Parser().Parse("""{"schemaVersion":"0.4.0"}""").Ids![type];
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moved = mover.Move(text);

        // Then
        JsonNode.Parse(moved)!["ids"]?[type].Should().BeNull();
        EngineServices.Parser().Parse(moved).Ids![type].Should().Be(shipped);
    }

    [Theory]
    [ClassData(typeof(MovedMintEntries))]
    public void AMintEntry_WhenMoved_ShouldBecomeItsRecordsMintLessItsPrefixOrAsItIsWhenNotAnObject(string type, string entry, string expected)
    {
        // Given
        var text = $$$"""{"schemaVersion":"0.4.0","mint":{"{{{type}}}":{{{entry}}}}}""";
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moved = mover.Move(text);

        // Then
        var record = JsonNode.Parse(moved)!["ids"]![type]!.AsObject();
        record.ContainsKey("mint").Should().BeTrue();
        JsonNode.DeepEquals(record["mint"], JsonNode.Parse(expected)).Should().BeTrue(record.ToJsonString());
    }

    [Fact]
    public void EveryOldIdSetting_WhenMoved_ShouldLeaveNoMintKeyAndNoIdentifiersMemberForAnIdType()
    {
        // Given
        const string text = """
            {
              "schemaVersion": "0.4.0",
              "identifiers": {
                "claim": "^B-[0-9]{3}[a-z]?$",
                "constraint": "^CON-[0-9]+$",
                "openQuestion": "^Q-[0-9]+$",
                "task": "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$",
                "scenario": "^S-[0-9]{4}$",
                "decision": "^D-[0-9]{4}$",
                "adr": "^ADR-[0-9]{4}$",
                "lesson": "^L-[0-9]{4}$"
              },
              "mint": { "$comment": "synthetic", "adr": { "strategy": "nanoid", "prefix": "ADR-" } }
            }
            """;
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moved = JsonNode.Parse(mover.Move(text))!.AsObject();

        // Then
        moved.Should().NotContainKey("mint");
        moved["identifiers"]!.AsObject().Select(static member => member.Key).Should().Equal("task");
    }

    [Theory]
    [ClassData(typeof(ManifestKeyEncodings))]
    public void AManifestInASupportedEncoding_WhenItsIdSettingsMove_ShouldKeepItsBomItsEncodingAndEveryOtherMemberAsWritten(string name, bool bom)
    {
        // Given
        var encoding = Encoding.GetEncoding(name);
        byte[] preamble = bom ? encoding.GetPreamble() : [];
        const string text = "{\r\n  \"$comment\": \"😀 kept\",\r\n  \"schemaVersion\" : \"0.4.0\",\r\n  \"identifiers\": {\r\n"
            + "    \"task\": \"^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$\",\r\n    \"claim\": \"^B-[0-9]{4}$\",\r\n"
            + "    \"feature\": \"^F[0-9]+$\"\r\n  },\r\n  \"mint\": { \"adr\": { \"strategy\": \"nanoid\" } },\r\n"
            + "  \"rules\": {\"SPEC4004\" :  \"warning\"}\r\n}\r\n";
        byte[] bytes = [.. preamble, .. encoding.GetBytes(text)];
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moved = mover.Move(bytes);

        // Then
        moved.AsSpan(0, preamble.Length).ToArray().Should().Equal(preamble);
        var written = encoding.GetString(moved.AsSpan(preamble.Length));
        foreach (var kept in new[]
                 {
                     "\"$comment\": \"😀 kept\"",
                     "\"schemaVersion\" : \"0.4.0\"",
                     "\"task\": \"^(?<epic>[0-9]{4})-(?<sequence>[0-9]{3})$\"",
                     "\"feature\": \"^F[0-9]+$\"",
                     "\"rules\": {\"SPEC4004\" :  \"warning\"}",
                 })
        {
            written.Should().Contain(kept);
        }

        var after = JsonNode.Parse(written)!.AsObject();
        after["ids"]!["claim"]!["id"]!.GetValue<string>().Should().Be("[0-9]{4}");
        after.Remove("ids");
        var expected = JsonNode.Parse(text)!.AsObject();
        expected.Remove("mint");
        expected["identifiers"]!.AsObject().Remove("claim");
        JsonNode.DeepEquals(after, expected).Should().BeTrue(written);
    }

    [Theory]
    [ClassData(typeof(UnmovableIdSettings))]
    public void ASettingTheMoveCannotMakeExactly_WhenMoved_ShouldThrowOneRejectionNamingEachSuchMember(
        string text,
        string[] members,
        string[] unwritten)
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes(text);
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moveText = () => mover.Move(text);
        var moveBytes = () => mover.Move(bytes);

        // Then
        var message = moveText.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        foreach (var member in members)
        {
            message.Should().MatchRegex($@"(?<![\w.]){Regex.Escape(member)}(?!\.?\w)", member);
        }

        foreach (var member in unwritten)
        {
            message.Should().NotContain(member);
        }

        moveBytes.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().Be(message);
        bytes.Should().Equal(Encoding.UTF8.GetBytes(text));
    }

    [Theory]
    [ClassData(typeof(CaseDistinctClaimAndScenarioGrammars))]
    public void ClaimAndScenarioGrammarsWhoseHeadsDifferOnlyInCase_WhenMovedWithTheTagCaseNotIgnored_ShouldBecomeTwoRecords(string text)
    {
        // Given
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var moved = JsonNode.Parse(mover.Move(text))!["ids"]!;

        // Then
        moved["claim"]!["plain"]!.GetValue<string>().Should().Be("X-{id}");
        moved["scenario"]!["plain"]!.GetValue<string>().Should().Be("x-{id}");
    }

    [Theory]
    [ClassData(typeof(OldIdSettingsBesideAnIdsThatIsNotAnObject))]
    public void OldIdSettingsBesideAnIdsThatIsNotAnObject_WhenMoved_ShouldReturnTheSameTextAndTheSameBytes(string text)
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes(text);
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var movedText = mover.Move(text);
        var movedBytes = mover.Move(bytes);

        // Then
        movedText.Should().BeSameAs(text);
        movedBytes.Should().BeSameAs(bytes);
    }

    [Theory]
    [ClassData(typeof(ManifestsWithNoOldIdSetting))]
    public void AManifestHoldingNoOldIdSetting_WhenMoved_ShouldReturnTheSameTextAndTheSameBytes(string text)
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes(text);
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var movedText = mover.Move(text);
        var movedBytes = mover.Move(bytes);

        // Then
        movedText.Should().BeSameAs(text);
        movedBytes.Should().BeSameAs(bytes);
    }

    [Theory]
    [ClassData(typeof(OldIdSettingsBelowZeroFourZero))]
    public void OldIdSettingsUnderAPinBelowZeroFourZero_WhenMoved_ShouldReturnTheSameTextAndTheSameBytes(string text)
    {
        // Given
        var bytes = Encoding.UTF8.GetBytes(text);
        var mover = new ManifestIdMover(new ManifestPinEditor(EngineServices.Parser(), EngineServices.Versions()), EngineServices.Versions());

        // When
        var movedText = mover.Move(text);
        var movedBytes = mover.Move(bytes);

        // Then
        movedText.Should().BeSameAs(text);
        movedBytes.Should().BeSameAs(bytes);
    }
}
